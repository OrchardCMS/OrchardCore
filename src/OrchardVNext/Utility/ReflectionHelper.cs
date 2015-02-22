using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace OrchardVNext.Utility {
    public class ReflectionHelper<T> {
        private static readonly ConcurrentDictionary<string, Delegate> _getterCache =
            new ConcurrentDictionary<string, Delegate>();

        public delegate TProperty PropertyGetterDelegate<out TProperty>(T target);

        /// <summary>
        /// Gets property info out of a Lambda.
        /// </summary>
        /// <typeparam name="TProperty">The return type of the Lambda.</typeparam>
        /// <param name="expression">The Lambda expression.</param>
        /// <returns>The property info.</returns>
        public static PropertyInfo GetPropertyInfo<TProperty>(Expression<Func<T, TProperty>> expression) {
            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null) {
                throw new InvalidOperationException("Expression is not a member expression.");
            }
            var propertyInfo = memberExpression.Member as PropertyInfo;
            if (propertyInfo == null) {
                throw new InvalidOperationException("Expression is not for a property.");
            }
            return propertyInfo;
        }

        /// <summary>
        /// Gets a delegate from a property expression.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="targetExpression">The property expression.</param>
        /// <returns>The delegate.</returns>
        public static PropertyGetterDelegate<TProperty> GetGetter<TProperty>(
            Expression<Func<T, TProperty>> targetExpression) {

            var propertyInfo = GetPropertyInfo(targetExpression);
            return (PropertyGetterDelegate<TProperty>) _getterCache
                .GetOrAdd(propertyInfo.Name,
                    s => propertyInfo.GetGetMethod().CreateDelegate(typeof(PropertyGetterDelegate<TProperty>)));
        }
    }
}