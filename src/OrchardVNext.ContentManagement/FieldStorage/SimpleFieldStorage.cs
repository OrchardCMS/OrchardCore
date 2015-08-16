using System;
using System.Globalization;
using System.Xml;
using System.Reflection;

namespace OrchardVNext.ContentManagement.FieldStorage {
    public class SimpleFieldStorage : IFieldStorage {
        public SimpleFieldStorage(Func<string, Type, string> getter, Action<string, Type, string> setter) {
            Getter = getter;
            Setter = setter;
        }

        public Func<string, Type, string> Getter { get; set; }
        public Action<string, Type, string> Setter { get; set; }

        public T Get<T>(string name) {
            var value = Getter(name, typeof(T));
            if(string.IsNullOrEmpty(value)) {
                return default(T);
            }

            var t = typeof (T);

            // the T is nullable, convert using underlying type
            if (t.GetTypeInfo().IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                t = Nullable.GetUnderlyingType(t);
            }

            // using a special case for DateTime as it would lose milliseconds otherwise
            if (typeof(T) == typeof(DateTime)) {
                var result = XmlConvert.ToDateTime(value, XmlDateTimeSerializationMode.Utc);
                return (T) (object)result;
            }

            return (T)Convert.ChangeType(value, t, CultureInfo.InvariantCulture);
        }

        public void Set<T>(string name, T value) {
            
            // using a special case for DateTime as it would lose milliseconds otherwise
            if (typeof(T) == typeof(DateTime)) {
                var text = ((DateTime)(object)value).ToString("o", CultureInfo.InvariantCulture);
                Setter(name, typeof(T), text);
            }
            else {
                Setter(name, typeof (T), Convert.ToString(value, CultureInfo.InvariantCulture));
            }
        }
    }
}