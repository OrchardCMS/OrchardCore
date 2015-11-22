using System;

namespace Orchard.Validation
{
    public class Argument
    {
        [ContractAnnotation("halt <= condition: true")]
        public static void Validate(bool condition, string name)
        {
            if (!condition)
            {
                throw new ArgumentException("Invalid argument", name);
            }
        }

        [ContractAnnotation("halt <= condition: true")]
        public static void Validate(bool condition, string name, string message)
        {
            if (!condition)
            {
                throw new ArgumentException(message, name);
            }
        }


        [ContractAnnotation("value:null => fail")]
        public static void ThrowIfNull<T>(T value, string name) where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(name);
            }
        }

        [ContractAnnotation("value:null => fail")]
        public static void ThrowIfNull<T>(T value, string name, string message) where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(name, message);
            }
        }

        [ContractAnnotation("value:null => fail")]
        public static void ThrowIfNullOrEmpty(string value, string name)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("Argument must be a non empty string", name);
            }
        }

        [ContractAnnotation("value:null => fail")]
        public static void ThrowIfNullOrEmpty(string value, string name, string message)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException(message, name);
            }
        }

        /// <summary>
        /// Describes dependency between method input and output
        /// </summary>
        /// <syntax>
        /// <p>Function Definition Table syntax:</p>
        /// <list>
        /// <item>FDT      ::= FDTRow [;FDTRow]*</item>
        /// <item>FDTRow   ::= Input =&gt; Output | Output &lt;= Input</item>
        /// <item>Input    ::= ParameterName: Value [, Input]*</item>
        /// <item>Output   ::= [ParameterName: Value]* {halt|stop|void|nothing|Value}</item>
        /// <item>Value    ::= true | false | null | notnull | canbenull</item>
        /// </list>
        /// If method has single input parameter, it's name could be omitted.<br/>
        /// Using <c>halt</c> (or <c>void</c>/<c>nothing</c>, which is the same)
        /// for method output means that the methos doesn't return normally.<br/>
        /// <c>canbenull</c> annotation is only applicable for output parameters.<br/>
        /// You can use multiple <c>[ContractAnnotation]</c> for each FDT row,
        /// or use single attribute with rows separated by semicolon.<br/>
        /// </syntax>
        /// <examples><list>
        /// <item><code>
        /// [ContractAnnotation("=> halt")]
        /// public void TerminationMethod()
        /// </code></item>
        /// <item><code>
        /// [ContractAnnotation("halt &lt;= condition: false")]
        /// public void Assert(bool condition, string text) // regular assertion method
        /// </code></item>
        /// <item><code>
        /// [ContractAnnotation("s:null => true")]
        /// public bool IsNullOrEmpty(string s) // string.IsNullOrEmpty()
        /// </code></item>
        /// <item><code>
        /// // A method that returns null if the parameter is null, and not null if the parameter is not null
        /// [ContractAnnotation("null => null; notnull => notnull")]
        /// public object Transform(object data)
        /// </code></item>
        /// <item><code>
        /// [ContractAnnotation("s:null=>false; =>true,result:notnull; =>false, result:null")]
        /// public bool TryParse(string s, out Person result)
        /// </code></item>
        /// </list></examples>
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
        public sealed class ContractAnnotationAttribute : Attribute
        {
            public ContractAnnotationAttribute(string contract)
              : this(contract, false)
            { }

            public ContractAnnotationAttribute(string contract, bool forceFullStates)
            {
                Contract = contract;
                ForceFullStates = forceFullStates;
            }

            public string Contract { get; private set; }
            public bool ForceFullStates { get; private set; }
        }
    }
}