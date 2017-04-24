namespace Orchard.Environment.Cache
{
    public struct CacheContextEntry
    {
        public CacheContextEntry(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; set; }
        public string Value { get; set; }
    }
}
