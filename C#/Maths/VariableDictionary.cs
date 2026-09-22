using System;
using System.Collections.Generic;
using Maths.Properties;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Maths
{
    /// <summary>
    /// Class representing a collection of variable names and values.
    /// </summary>
    /// <remarks>
    /// Variable names can only contain letters, numbers and symbols are not allowed.
    /// </remarks>
    [Serializable]
    public class VariableDictionary : Dictionary<string, double>
    {
        private Calculator _calculator;

        /// <summary>Initializes a new instance of the <see cref="VariableDictionary"/> class.</summary>
        /// <param name="calculator">The evaluator.</param>
        internal VariableDictionary(Calculator calculator)
            : base(StringComparer.OrdinalIgnoreCase)
        {
            _calculator = calculator;
            base.Add(Calculator.AnswerVariable, 0);
            base.Add("pi", Math.PI);
            base.Add("e", Math.E);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableDictionary"/> class.
        /// </summary>
        /// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo"/> object containing the information required to serialize the <see cref="T:System.Collections.Generic.Dictionary`2"/>.</param>
        /// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext"/> structure containing the source and destination of the serialized stream associated with the <see cref="T:System.Collections.Generic.Dictionary`2"/>.</param>
        protected VariableDictionary(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        /// <summary>Adds the specified variable and value to the dictionary.</summary>
        /// <param name="name">The name of the variable to add.</param>
        /// <param name="value">The value of the variable.</param>
        /// <exception cref="ArgumentNullException">When variable name is null.</exception>
        /// <exception cref="ArgumentException">When variable name contains non-letters or the name exists in the <see cref="global::Calculator.Functions"/> list.</exception>
        /// <seealso cref="global::Calculator"/>
        /// <seealso cref="global::Calculator.Variables"/>
        /// <seealso cref="global::Calculator.Functions"/>
        public new void Add(string name, double value)
        {
            Validate(name);
            base.Add(name, value);
        }

        private void Validate(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            if (_calculator.IsFunction(name))
                throw new ArgumentException(
                    string.Format(CultureInfo.CurrentCulture,
                        Resources.VariableNameConflict, name), "name");

            for (int i = 0; i < name.Length; i++)
                if (!char.IsLetter(name[i]))
                    throw new ArgumentException(Resources.VariableNameContainsLetters, "name");
        }

        /// <summary>
        /// Implements the <see cref="T:System.Runtime.Serialization.ISerializable"/> interface and returns the data needed to serialize the <see cref="T:System.Collections.Generic.Dictionary`2"/> instance.
        /// </summary>
        /// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo"/> object that contains the information required to serialize the <see cref="T:System.Collections.Generic.Dictionary`2"/> instance.</param>
        /// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext"/> structure that contains the source and destination of the serialized stream associated with the <see cref="T:System.Collections.Generic.Dictionary`2"/> instance.</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

    }
}
