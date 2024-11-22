namespace OrchardCore.Search.Elasticsearch;

public class ElasticsearchResult
{
    public List<ElasticsearchRecord> TopDocs { get; set; }

    public List<ElasticsearchRecord> Fields { get; set; }

    public long Count { get; set; }
}

public class ElasticsearchRecord
{
    public Dictionary<string, object> Data { get; set; }

    public IReadOnlyDictionary<string, IReadOnlyCollection<string>> Highlights { get; set; }

    public double? Score { get; set; }

    public ElasticsearchRecord()
    {
    }

    public ElasticsearchRecord(Dictionary<string, object> data)
    {
        Data = data;
    }

    public bool TryGetDataValue(string key, out object value)
    {
        if (Data == null)
        {
            value = null;

            return false;
        }

        return Data.TryGetValue(key, out value);
    }

    public bool TryGetDataValue<T>(string key, out T value)
    {
        if (Data == null)
        {
            value = default;

            return false;
        }

        if (Data.TryGetValue(key, out var obj))
        {
            if (obj is T typedValue)
            {
                value = typedValue;

                return true;
            }

            if (typeof(T) == typeof(string))
            {
                value = (T)(object)obj?.ToString();

                return true;
            }

            // Handle nullable types (e.g., Nullable<T>).
            if (Nullable.GetUnderlyingType(typeof(T)) != null)
            {
                // If the object is null, assign the default value for the nullable type.
                if (obj == null)
                {
                    value = default;

                    return true; // Return true for null values if T is nullable.
                }
            }

            try
            {
                value = (T)Convert.ChangeType(obj, typeof(T));

                return true;
            }
            catch
            {
            }
        }

        value = default;

        return false;
    }
}
