using System;
using JetBrains.Annotations;

namespace OrchardVNext.Validation {
    public class Argument {
        [ContractAnnotation("halt <= condition: true")]
        public static void Validate(bool condition, [InvokerParameterName]string name) {
            if (!condition) {
                throw new ArgumentException("Invalid argument", name);
            }
        }

        [ContractAnnotation("halt <= condition: true")]
        public static void Validate(bool condition, [InvokerParameterName]string name, string message) {
            if (!condition) {
                throw new ArgumentException(message, name);
            }
        }
        

        [ContractAnnotation("value:null => fail")]
        public static void ThrowIfNull<T>(T value, [InvokerParameterName]string name) where T : class {
            if (value == null) {
                throw new ArgumentNullException(name);
            }
        }

        [ContractAnnotation("value:null => fail")]
        public static void ThrowIfNull<T>(T value, [InvokerParameterName]string name, string message) where T : class {
            if (value == null) {
                throw new ArgumentNullException(name, message);
            }
        }

        [ContractAnnotation("value:null => fail")]
        public static void ThrowIfNullOrEmpty(string value, [InvokerParameterName]string name) {
            if (string.IsNullOrEmpty(value)) {
                throw new ArgumentException("Argument must be a non empty string", name);
            }
        }

        [ContractAnnotation("value:null => fail")]
        public static void ThrowIfNullOrEmpty(string value, [InvokerParameterName]string name, string message) {
            if (string.IsNullOrEmpty(value)) {
                throw new ArgumentException(message, name);
            }
        }
    }
}
