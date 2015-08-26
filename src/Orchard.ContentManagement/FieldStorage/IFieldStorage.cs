namespace Orchard.ContentManagement.FieldStorage {
    public interface IFieldStorage {
        T Get<T>(string name);
        void Set<T>(string name, T value);
    }
    public static class FieldStorageExtension{
        public static T Get<T>(this IFieldStorage storage) {
            return storage.Get<T>(null);
        }
        public static void Set<T>(this IFieldStorage storage, T value) {
            storage.Set(null, value);
        }
    }
}