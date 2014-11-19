using System;
using JetBrains.Annotations;

namespace OrchardVNext.Validation {
    public class Argument {
        [AssertionMethod]
        public static void Validate([AssertionCondition(AssertionConditionType.IS_TRUE)] bool condition, [InvokerParameterName]string name) {
            if (!condition) {
                throw new ArgumentException("Invalid argument", name);
            }
        }

        [AssertionMethod]
        public static void Validate([AssertionCondition(AssertionConditionType.IS_TRUE)]bool condition, [InvokerParameterName]string name, string message) {
            if (!condition) {
                throw new ArgumentException(message, name);
            }
        }

        [AssertionMethod]
        public static void ThrowIfNull<T>([AssertionCondition(AssertionConditionType.IS_NOT_NULL)]T value, [InvokerParameterName]string name) where T : class {
            if (value == null) {
                throw new ArgumentNullException(name);
            }
        }

        [AssertionMethod]
        public static void ThrowIfNull<T>([AssertionCondition(AssertionConditionType.IS_NOT_NULL)]T value, [InvokerParameterName]string name, string message) where T : class {
            if (value == null) {
                throw new ArgumentNullException(name, message);
            }
        }

        [AssertionMethod]
        public static void ThrowIfNullOrEmpty([AssertionCondition(AssertionConditionType.IS_NOT_NULL)]string value, [InvokerParameterName]string name) {
            if (string.IsNullOrEmpty(value)) {
                throw new ArgumentException("Argument must be a non empty string", name);
            }
        }

        [AssertionMethod]
        public static void ThrowIfNullOrEmpty([AssertionCondition(AssertionConditionType.IS_NOT_NULL)]string value, [InvokerParameterName]string name, string message) {
            if (string.IsNullOrEmpty(value)) {
                throw new ArgumentException(message, name);
            }
        }
    }
}
