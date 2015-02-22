using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Xml;
using System.Xml.Linq;
using System.Reflection;
using OrchardVNext.Utility;

namespace OrchardVNext.ContentManagement {
    public static class XmlHelper {
        /// <summary>
        /// Like Add, but chainable.
        /// </summary>
        /// <param name="el">The parent element.</param>
        /// <param name="children">The elements to add.</param>
        /// <returns>Itself</returns>
        public static XElement AddEl(this XElement el, params XElement[] children) {
            el.Add(children.Cast<object>());
            return el;
        }

        /// <summary>
        /// Gets the string value of an attribute, and null if the attribute doesn't exist.
        /// </summary>
        /// <param name="el">The element.</param>
        /// <param name="name">The name of the attribute.</param>
        /// <returns>The string value of the attribute if it exists, null otherwise.</returns>
        public static string Attr(this XElement el, string name) {
            var attr = el.Attribute(name);
            return attr == null ? null : attr.Value;
        }

        /// <summary>
        /// Gets a typed value from an attribute.
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="el">The element.</param>
        /// <param name="name">The name of the attribute.</param>
        /// <returns>The attribute value</returns>
        public static T Attr<T>(this XElement el, string name) {

            var attr = el.Attribute(name);
            return attr == null ? default(T) : Parse<T>(attr.Value);
        }

        /// <summary>
        /// Sets an attribute value. This is chainable.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="el">The element.</param>
        /// <param name="name">The attribute name.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>Itself</returns>
        public static XElement Attr<T>(this XElement el, string name, T value) {
            el.SetAttributeValue(name, ToString(value));
            return el;
        }

        /// <summary>
        /// Returns the text contents of a child element.
        /// </summary>
        /// <param name="el">The parent element.</param>
        /// <param name="name">The name of the child element.</param>
        /// <returns>The text for the child element, and null if it doesn't exist.</returns>
        public static string El(this XElement el, string name) {
            var childElement = el.Element(name);
            return childElement == null ? null : childElement.Value;
        }

        /// <summary>
        /// Creates and sets the value of a child element. This is chainable.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="el">The parent element.</param>
        /// <param name="name">The name of the child element.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>Itself</returns>
        public static XElement El<T>(this XElement el, string name, T value) {
            el.SetElementValue(name, value);
            return el;
        }

        /// <summary>
        /// Sets a property value from an attribute of the same name.
        /// </summary>
        /// <typeparam name="TTarget">The type of the target object.</typeparam>
        /// <typeparam name="TProperty">The type of the target property</typeparam>
        /// <param name="el">The element.</param>
        /// <param name="target">The target object.</param>
        /// <param name="targetExpression">The property expression.</param>
        /// <returns>Itself</returns>
        public static XElement FromAttr<TTarget, TProperty>(this XElement el, TTarget target,
            Expression<Func<TTarget, TProperty>> targetExpression) {

            if (target == null) return el;
            var propertyInfo = ReflectionHelper<TTarget>.GetPropertyInfo(targetExpression);
            var name = propertyInfo.Name;
            var attr = el.Attribute(name);

            if (attr == null) return el;
            propertyInfo.SetValue(target, el.Attr<TProperty>(name), null);
            return el;
        }

        /// <summary>
        /// Sets an attribute with the value of a property of the same name.
        /// </summary>
        /// <typeparam name="TTarget">The type of the object.</typeparam>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="el">The element.</param>
        /// <param name="target">The object.</param>
        /// <param name="targetExpression">The property expression.</param>
        /// <returns>Itself</returns>
        public static XElement ToAttr<TTarget, TProperty>(this XElement el, TTarget target,
            Expression<Func<TTarget, TProperty>> targetExpression) {

            if (target == null) return el;
            var propertyInfo = ReflectionHelper<TTarget>.GetPropertyInfo(targetExpression);
            var name = propertyInfo.Name;
            var val = (TProperty)propertyInfo.GetValue(target, null);

            el.Attr(name, ToString(val));
            return el;
        }

        /// <summary>
        /// Gets the text value of an element as the specified type.
        /// </summary>
        /// <typeparam name="TValue">The type to parse the element as.</typeparam>
        /// <param name="el">The element.</param>
        /// <returns>The value of the element as type TValue.</returns>
        public static TValue Val<TValue>(this XElement el) {
            return Parse<TValue>(el.Value);
        }

        /// <summary>
        /// Sets the value of an element.
        /// </summary>
        /// <typeparam name="TValue">The type of the value to set.</typeparam>
        /// <param name="el">The element.</param>
        /// <param name="value">The value.</param>
        /// <returns>The element.</returns>
        public static XElement Val<TValue>(this XElement el, TValue value) {
            el.SetValue(ToString(value));
            return el;
        }

        /// <summary>
        /// Serializes the provided value as a string.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>The string representation of the value.</returns>
        public static string ToString<T>(T value) {
            var type = typeof(T);
            if (type == typeof(string)) {
                return Convert.ToString(value);
            }
            if ((!type.GetTypeInfo().IsValueType || Nullable.GetUnderlyingType(type) != null) &&
                value == null && 
                type != typeof(string)) {

                return "null";
            }

            if (type == typeof(DateTime) || type == typeof(DateTime?)) {
                return XmlConvert.ToString(Convert.ToDateTime(value),
                    XmlDateTimeSerializationMode.Utc);
            }

            if (type == typeof(bool) ||
                type == typeof(bool?)) {
                return Convert.ToBoolean(value) ? "true" : "false";
            }

            if (type == typeof(int) ||
                type == typeof(int?) ||
                type == typeof(long) ||
                type == typeof(long?)) {

                return Convert.ToInt64(value).ToString(CultureInfo.InvariantCulture);
            }

            if (type == typeof(double) ||
                type == typeof(double?)) {

                var doubleValue = (double)(object)value;
                if (double.IsPositiveInfinity(doubleValue)) {
                    return "infinity";
                }
                if (double.IsNegativeInfinity(doubleValue)) {
                    return "-infinity";
                }
                return doubleValue.ToString(CultureInfo.InvariantCulture);
            }

            if (type == typeof(float) ||
                type == typeof(float?)) {

                var floatValue = (float)(object)value;
                if (float.IsPositiveInfinity(floatValue)) {
                    return "infinity";
                }
                if (float.IsNegativeInfinity(floatValue)) {
                    return "-infinity";
                }
                return floatValue.ToString(CultureInfo.InvariantCulture);
            }

            if (type == typeof(decimal) ||
                type == typeof(decimal?)) {

                var decimalValue = Convert.ToDecimal(value);
                return decimalValue.ToString(CultureInfo.InvariantCulture);
            }

            if (type.GetTypeInfo().IsEnum) {
                return value.ToString();
            }

            throw new NotSupportedException(String.Format("Could not handle type {0}", type.Name));
        }

        /// <summary>
        /// Parses a string value as the provided type.
        /// </summary>
        /// <typeparam name="T">The destination type</typeparam>
        /// <param name="value">The string representation of the value to parse.</param>
        /// <returns>The parsed value with type T.</returns>
        public static T Parse<T>(string value) {
            var type = typeof(T);

            if (type == typeof(string)) {
                return (T)(object)value;
            }
            if (value == null ||
                "null".Equals(value, StringComparison.Ordinal) &&
                ((!type.GetTypeInfo().IsValueType || Nullable.GetUnderlyingType(type) != null))) {

                return default(T);
            }

            if ("infinity".Equals(value, StringComparison.Ordinal)) {
                if (type == typeof(float) || type == typeof(float?)) return (T)(object)float.PositiveInfinity;
                if (type == typeof(double) || type == typeof(double?)) return (T)(object)double.PositiveInfinity;
                throw new NotSupportedException(String.Format("Infinity not supported for type {0}", type.Name));
            }
            if ("-infinity".Equals(value, StringComparison.Ordinal)) {
                if (type == typeof(float)) return (T)(object)float.NegativeInfinity;
                if (type == typeof(double)) return (T)(object)double.NegativeInfinity;
                throw new NotSupportedException(String.Format("Infinity not supported for type {0}", type.Name));
            }
            if (type == typeof(int) || type == typeof(int?)) {
                return (T)(object)int.Parse(value, CultureInfo.InvariantCulture);
            }
            if (type == typeof(long) || type == typeof(long?)) {
                return (T)(object)long.Parse(value, CultureInfo.InvariantCulture);
            }
            if (type == typeof(bool) || type == typeof(bool?)) {
                return (T)(object)value.Equals("true", StringComparison.Ordinal);
            }
            if (type == typeof(DateTime) || type == typeof(DateTime?)) {
                return (T)(object)XmlConvert.ToDateTime(value, XmlDateTimeSerializationMode.Utc);
            }
            if (type == typeof(double) || type == typeof(double?)) {
                return (T)(object)double.Parse(value, CultureInfo.InvariantCulture);
            }
            if (type == typeof(float) || type == typeof(float?)) {
                return (T)(object)float.Parse(value, CultureInfo.InvariantCulture);
            }
            if (type == typeof(decimal) || type == typeof(decimal?)) {
                return (T)(object)decimal.Parse(value, CultureInfo.InvariantCulture);
            }

            if (type.GetTypeInfo().IsEnum) {
                return (T)Enum.Parse(type, value);
            }

            throw new NotSupportedException(String.Format("Could not handle type {0}", type.Name));
        }

        /// <summary>
        /// Gives context to an XElement, enabling chained property operations.
        /// </summary>
        /// <typeparam name="TContext">The type of the context.</typeparam>
        /// <param name="el">The element.</param>
        /// <param name="context">The context.</param>
        /// <returns>The element with context.</returns>
        public static XElementWithContext<TContext> With<TContext>(this XElement el, TContext context) {
            return new XElementWithContext<TContext>(el, context);
        }

        /// <summary>
        /// A wrapper for XElement, with context, for strongly-typed manipulation
        /// of an XElement.
        /// </summary>
        /// <typeparam name="TContext">The type of the context.</typeparam>
        public class XElementWithContext<TContext> {
            public XElementWithContext(XElement element, TContext context) {
                Element = element;
                Context = context;
            }

            public XElement Element { get; private set; }
            public TContext Context { get; private set; }

            public static implicit operator XElement(XElementWithContext<TContext> elementWithContext) {
                return elementWithContext.Element;
            }

            /// <summary>
            /// Replaces the current context with a new one, enabling chained action on different objects.
            /// </summary>
            /// <typeparam name="TNewContext">The type of the new context.</typeparam>
            /// <param name="context">The new context.</param>
            /// <returns>A new XElementWithContext, that has the new context.</returns>
            public XElementWithContext<TNewContext> With<TNewContext>(TNewContext context) {
                return new XElementWithContext<TNewContext>(Element, context);
            }

            /// <summary>
            /// Sets the value of a context property as an attribute of the same name on the element.
            /// </summary>
            /// <typeparam name="TProperty">The type of the property.</typeparam>
            /// <param name="targetExpression">The property expression.</param>
            /// <returns>Itself</returns>
            public XElementWithContext<TContext> ToAttr<TProperty>(
                Expression<Func<TContext, TProperty>> targetExpression) {
                Element.ToAttr(Context, targetExpression);
                return this;
            }

            /// <summary>
            /// Gets an attribute on the element and sets the property of the same name on the context with its value.
            /// </summary>
            /// <typeparam name="TProperty">The type of the property.</typeparam>
            /// <param name="targetExpression">The property expression.</param>
            /// <returns>Itself</returns>
            public XElementWithContext<TContext> FromAttr<TProperty>(
                Expression<Func<TContext, TProperty>> targetExpression) {
                Element.FromAttr(Context, targetExpression);
                return this;
            }

            /// <summary>
            /// Evaluates an attribute from an expression.
            /// It's a nice strongly-typed way to read attributes.
            /// </summary>
            /// <typeparam name="TProperty">The type of the property.</typeparam>
            /// <param name="expression">The property expression.</param>
            /// <returns>The attribute, ready to be cast.</returns>
            public TProperty Attr<TProperty>(Expression<Func<TContext, TProperty>> expression) {
                var propertyInfo = ReflectionHelper<TContext>.GetPropertyInfo(expression);
                var name = propertyInfo.Name;
                return Element.Attr<TProperty>(name);
            }
        }
    }
}