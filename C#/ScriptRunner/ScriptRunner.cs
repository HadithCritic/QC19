using System;
using System.Text;
using System.IO;
using System.Reflection;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Security;
//using System.IO.IsolatedStorage;
//using System.Security.Permissions;

public static class ScriptRunner
{
    static ScriptRunner()
    {
        if (!Directory.Exists(Globals.SCRIPTS_FOLDER))
        {
            Directory.CreateDirectory(Globals.SCRIPTS_FOLDER);
        }
    }

    /// <summary>
    /// Load a C# script fie
    /// </summary>
    /// <param name="path">file to load</param>
    /// <returns>file content</returns>
    public static string LoadScript(string filename)
    {
        StringBuilder str = new StringBuilder();
        string path = Globals.SCRIPTS_FOLDER + Path.DirectorySeparatorChar + filename;
        if (File.Exists(path))
        {
            using (StreamReader reader = File.OpenText(path))
            {
                string line = null;
                while ((line = reader.ReadLine()) != null)
                {
                    str.AppendLine(line);
                }
            }
        }
        return str.ToString();
    }

    /// <summary>
    /// Save a C# script fie
    /// </summary>
    /// <param name="filename">file to save</param>
    /// <returns></returns>
    public static void SaveScript(string filename, string text)
    {
        if (!String.IsNullOrEmpty(filename))
        {
            string[] parts = filename.Split('~');
            if (parts.Length > 1)
            {
                filename = parts[0] + ".cs";
            }
            filename = filename.Insert(filename.Length - 3, "~" + DateTime.Now.ToString("yyyyMMdd_hhmmss"));

            string path = Globals.SCRIPTS_FOLDER + Path.DirectorySeparatorChar + filename;
            if (Directory.Exists(Globals.SCRIPTS_FOLDER))
            {
                using (StreamWriter writer = new StreamWriter(path, false, Encoding.Unicode))
                {
                    writer.WriteLine(text);
                    writer.Close();
                }
            }
        }
    }

    /// <summary>
    /// Compiles the source_code 
    /// </summary>
    /// <param name="source_code">source_code must implements IScript interface</param>
    /// <returns>compiled Assembly</returns>
    public static CompilerResults CompileCode(string source_code)
    {
        CSharpCodeProvider provider = new CSharpCodeProvider();
        if (provider != null)
        {
            CompilerParameters options = new CompilerParameters();
            if (options != null)
            {
                options.GenerateExecutable = false;  // generate a Class Library assembly
                options.GenerateInMemory = true;     // so we don't have to delete it from disk

                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (Assembly assembly in assemblies)
                {
                    options.ReferencedAssemblies.Add(assembly.Location);
                }

                return provider.CompileAssemblyFromSource(options, source_code);
            }
            return null;
        }
        return null;
    }

    /// <summary>
    /// Execute the IScriptRunner.Run method in the compiled_assembly
    /// </summary>
    /// <param name="compiled_assembly">compiled assembly</param>
    /// <param name="args">method arguments</param>
    /// <returns>object returned</returns>
    public static object Run(Assembly compiled_assembly, object[] args, PermissionSet permission_set)
    {
        if (compiled_assembly != null)
        {
            // security is not implemented yet!
            // using Utilties.PrivateStorage can save but not display in Notepad
            // plus the output is saved in C:\Users\<user>\AppData\Local\IsolatedStorage\...
            // no control over where to save make QuranCode unportable applicaton
            //// restrict code security
            //???permission_set.PermitOnly();

            foreach (Type type in compiled_assembly.GetExportedTypes())
            {
                foreach (Type interface_type in type.GetInterfaces())
                {
                    if (interface_type == typeof(IScriptRunner))
                    {
                        ConstructorInfo constructor = type.GetConstructor(System.Type.EmptyTypes);
                        if ((constructor != null) && (constructor.IsPublic))
                        {
                            // construct object using default constructor
                            IScriptRunner obj = constructor.Invoke(null) as IScriptRunner;
                            if (obj != null)
                            {
                                return obj.Run(args);
                            }
                            throw new Exception("Invalid C# code!");
                        }
                        throw new Exception("No default constructor was found!");
                    }
                    throw new Exception("IScriptRunner is not implemented!");
                }
            }

            // revert security restrictions
            CodeAccessPermission.RevertPermitOnly();
        }
        return null;
    }

    /// <summary>
    /// Execute a public static method_name(args) in compiled_assembly
    /// </summary>
    /// <param name="compiled_assembly">compiled assembly</param>
    /// <param name="methode_name">method to execute</param>
    /// <param name="args">method arguments</param>
    /// <returns>method execution result</returns>
    public static object ExecuteStaticMethod(Assembly compiled_assembly, string methode_name, object[] args)
    {
        if (compiled_assembly != null)
        {
            foreach (Type type in compiled_assembly.GetTypes())
            {
                foreach (MethodInfo method in type.GetMethods())
                {
                    if (method.Name == methode_name)
                    {
                        if ((method != null) && (method.IsPublic) && (method.IsStatic))
                        {
                            return method.Invoke(null, args);
                        }
                        throw new Exception("Cannot invoke method :" + methode_name);
                    }
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Execute a public method_name(args) in compiled_assembly
    /// </summary>
    /// <param name="compiled_assembly">compiled assembly</param>
    /// <param name="methode_name">method to execute</param>
    /// <param name="args">method arguments</param>
    /// <returns>method execution result</returns>
    public static object ExecuteInstanceMethod(Assembly compiled_assembly, string methode_name, object[] args)
    {
        if (compiled_assembly != null)
        {
            foreach (Type type in compiled_assembly.GetTypes())
            {
                foreach (MethodInfo method in type.GetMethods())
                {
                    if (method.Name == methode_name)
                    {
                        if ((method != null) && (method.IsPublic))
                        {
                            object obj = Activator.CreateInstance(type, null);
                            return method.Invoke(obj, args);
                        }
                        throw new Exception("Cannot invoke method :" + methode_name);
                    }
                }
            }
        }
        return null;
    }
}

// Usage from MainForm
//if (!ScriptTextBox.Visible)
//{
//    ScriptTextBox.Text = ScriptRunner.LoadScript(@"Scripts\Sample.cs");
//    ScriptTextBox.Visible = true;
//}
//else // if visible
//{
//    if (m_client != null)
//    {
//        if (m_client.Selection != null)
//        {
//            string source_code = ScriptTextBox.Text;
//            if (source_code.Length > 0)
//            {
//                Assembly compiled_assembly = ScriptRunner.CompileCode(source_code);
//                if (compiled_assembly != null)
//                {
//                    object[] args = new object[] { 7, 29 };
//                    object result = ScriptRunner.Run(compiled_assembly, args);
//                    MessageBox.Show(args[0].ToString() + " + " + args[1].ToString() + " = " + result.ToString(), Application.ProductName);

//                    object[] args = new object[] { m_client, m_client.Selection.Verses, "" };
//                    object result = ScriptRunner.Run(compiled_assembly, args);

//                    object[] args = new object[] { m_client, m_client.Selection.Verses, "" };
//                    object result = ScriptRunner.Run(compiled_assembly, args);

//                    // here is how to call a non-static public method with prameters
//                    object[] args = new object[] { m_client, m_client.Selection.Verses, "" };
//                    object result = ScriptRunner.ExecuteClassMethod(compiled_assembly, "PublicScriptMethod", args);

//                    // here is how to call a non-static parameter-less method
//                    object[] args = null;
//                    object result = ScriptRunner.ExecuteClassMethod(compiled_assembly, "NonStaticScriptMethod", args);

//                    // here is how to call a non-static parameter-less method
//                    object[] args = null;
//                    object result = ScriptRunner.ExecuteStaticMethod(compiled_assembly, "StaticScriptMethod", args);

//                    // and here is how pass arguments and get result back
//                    object[] args = new object[] { 7, 29 };
//                    object result = ScriptRunner.ExecuteInstanceMethod(compiled_assembly, "Multiply", args);
//                    MessageBox.Show(args[0].ToString() + " * " + args[1].ToString() + " = " + result.ToString(), Application.ProductName);
//                }
//            }
//            ScriptTextBox.Visible = false;
//        }
//    }
//}
