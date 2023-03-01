namespace OrchardCore.Environment.Cache
{
    public readonly struct CacheContextEntry
    {
        public CacheContextEntry(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; init; }
        public string Value { get; init; }
    }
}
