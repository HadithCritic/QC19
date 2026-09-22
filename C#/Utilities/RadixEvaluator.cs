using System;
using System.Collections;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.CSharp;

public static class RadixEvaluator
{
    private static CSharpCodeProvider provider = null;
    static RadixEvaluator()
    {
        provider = new CSharpCodeProvider();

        // get all System.Math class members
        // so user doesn't need to add "Math." before them
        // and make them case-insensitive (pi, Pi, PI)
        PopulateMathLibrary();
    }

    private static CompilerParameters CreateCompilerParameters()
    {
        var compilerParams = new CompilerParameters
        {
            CompilerOptions = "/target:library /optimize",
            GenerateExecutable = false,
            GenerateInMemory = true,
            IncludeDebugInformation = false
        };
        compilerParams.ReferencedAssemblies.Add("System.dll");
        return compilerParams;
    }
    private static string GenerateCode(string expression)
    {
        var source = new StringBuilder();
        var sw = new StringWriter(source);
        var options = new CodeGeneratorOptions();
        var myNamespace = new CodeNamespace("");
        myNamespace.Imports.Add(new CodeNamespaceImport("System"));
        var classDeclaration = new CodeTypeDeclaration { IsClass = true, Name = "RadixEvaluator", Attributes = MemberAttributes.Public };
        var myMethod = new CodeMemberMethod { Name = "Evaluate", ReturnType = new CodeTypeReference(typeof(double)), Attributes = MemberAttributes.Public };
        myMethod.Statements.Add(new CodeMethodReturnStatement(new CodeSnippetExpression("(" + expression + ")")));
        classDeclaration.Members.Add(myMethod);
        myNamespace.Types.Add(classDeclaration);
        provider.GenerateCodeFromNamespace(myNamespace, sw, options);
        sw.Flush();
        sw.Close();
        return source.ToString();
    }
    private static CompilerResults CompileCode(string source)
    {
        CompilerParameters parms = CreateCompilerParameters();
        return provider.CompileAssemblyFromSource(parms, source);
    }
    private static string RunCode(CompilerResults results)
    {
        Assembly executingAssembly = results.CompiledAssembly;
        object assemblyInstance = executingAssembly.CreateInstance("RadixEvaluator");
        return assemblyInstance.GetType().GetMethod("Evaluate").Invoke(assemblyInstance, new object[] { }).ToString();
    }
    public static string Evaluate(string expression)
    {
        return Evaluate(expression, Numbers.DEFAULT_RADIX);
    }
    public static string Evaluate(string expression, int radix)
    {
        string processed_expression = PrepareExpression(expression, radix);

        string source = GenerateCode(processed_expression);

        CompilerResults results = CompileCode(source);
        if ((results == null) || (results.Errors.Count != 0) || (results.CompiledAssembly == null))
        {
            return expression;
        }

        return RunCode(results);
    }

    private static Hashtable s_math_library = new Hashtable();
    private static void PopulateMathLibrary()
    {
        // get a reflected assembly of the System assembly
        Assembly systemAssembly = Assembly.GetAssembly(typeof(System.Math));
        try
        {
            // cannot call the entry method if the assembly is null
            if (systemAssembly != null)
            {
                // use reflection to get a reference to the Math class

                Module[] modules = systemAssembly.GetModules(false);
                Type[] types = modules[0].GetTypes();

                // loop through each class that was defined and look for the first occurrence of the Math class
                foreach (Type type in types)
                {
                    if (type.Name == "Math")
                    {
                        // get all of the members of the math class and map them to the same member
                        // name in uppercase
                        MemberInfo[] mis = type.GetMembers();
                        foreach (MemberInfo mi in mis)
                        {
                            if ((mi.Name.ToUpper() != "PI") && (mi.Name.ToUpper() != "E"))
                            {
                                s_math_library[mi.Name.ToUpper()] = mi.Name;
                            }
                        }
                    }
                    // if the entry point method does return in Int32, then capture it and return it
                }

                // if it got here, then there was no entry point method defined.  Tell user about it
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: An exception occurred while executing the script", ex);
        }
    }
    private static string PrepareExpression(string expression, int radix)
    {
        char[] separators = { '(', ')', '+', '-', '*', '/', '%', '\\', '^', '!' };
        string[] parts = expression.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        foreach (string part in parts)
        {
            bool in_math_library = s_math_library[part.ToUpper()] != null;

            // 1. consruct Math.Xxx() functions and Math.XXX constants
            if (in_math_library)
            {
                expression = expression.Replace(part, "Math." + s_math_library[part.ToUpper()]);
            }
            // 2. decode to base-10
            else
            {
                long base10_value;
                try
                {
                    base10_value = Radix.Decode(part, radix);

                    int pos = expression.IndexOf(part);
                    expression = expression.Remove(pos, part.Length);
                    expression = expression.Insert(pos, base10_value.ToString());
                }
                catch
                {
                    continue; // leave part as is
                }
            }
        }

        // Only allow small letters as constant and reserve capital letters for higher base systems
        // SPECIAL CASES: PI
        expression = expression.Replace("pi", "Math.PI");
        // SPECIAL CASE: Euler's Constant
        expression = expression.Replace("e", "Math.E");
        // SPECIAL CASE: Golden ratio
        expression = expression.Replace("phi", "1.61803398874989");
        // SPECIAL CASE: ^ power operator
        parts = expression.Split('^');
        // process simple case for now.
        if (parts.Length == 2)
        {
            double n1 = 0.0D;
            try
            {
                n1 = double.Parse(parts[0]);
            }
            catch
            {
                if (parts[0] == "e") n1 = Math.E;
                else if (parts[0] == "pi") n1 = Math.PI;
                else if (parts[0] == "phi") n1 = 1.61803398874989D;
                else if (parts[0] == "Math.E") n1 = Math.E;
                else if (parts[0] == "Math.PI") n1 = Math.PI;
            }

            double n2 = 0.0D;
            try
            {
                n2 = double.Parse(parts[1]);
            }
            catch
            {
                if (parts[1] == "e") n2 = Math.E;
                else if (parts[1] == "pi") n2 = Math.PI;
                else if (parts[1] == "phi") n2 = 1.61803398874989D;
                else if (parts[1] == "Math.E") n2 = Math.E;
                else if (parts[1] == "Math.PI") n2 = Math.PI;
            }

            expression = "Math.Pow(" + n1 + "," + n2 + ")";
        }
        // SPECIAL CASE: ! Factorial
        parts = expression.Split('!');
        // process simple case for now.
        if (parts.Length == 2)
        {
            long n;
            if (long.TryParse(parts[0], out n))
            {
                long factorial = 1L;
                for (long i = 1; i <= n; i++)
                {
                    factorial *= i;
                }
                expression = factorial.ToString();
            }
        }
        // SPECIAL CASE: double and int division
        expression = expression.Replace("/", "/(double)");
        expression = expression.Replace("\\", "/");

        return expression;
    }
}
