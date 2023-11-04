using System.Collections;
using System.Collections.Generic;
using System.Dynamic;

#nullable enable

namespace System.Text.Json.Nodes;

public class DynamicDictionary : DynamicObject, IDictionary<string, object?>
{
    internal readonly IDictionary<string, object?> _dictionary;

    public DynamicDictionary() => _dictionary = new Dictionary<string, object?>();

    public DynamicDictionary(IDictionary<string, object?> dictionary) => _dictionary = dictionary;

    public object? this[string key]
    {
        get
        {
            _dictionary.TryGetValue(key, out var result);
            return Wrap(result);
        }
        set
        {
            _dictionary[key] = Unwrap(value);
        }
    }

    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        result = this[binder.Name];
        return true;
    }

    public override bool TrySetMember(SetMemberBinder binder, object? value)
    {
        this[binder.Name] = value;
        return true;
    }

    public static object? Wrap(object? value)
    {
        if (value is IDictionary<string, object?> dictionary)
        {
            return new DynamicDictionary(dictionary);
        }

        return value;
    }

    public static object? Unwrap(object? value)
    {
        if (value is DynamicDictionary dictionary)
        {
            return dictionary._dictionary;
        }

        return value;
    }

    public void Add(string key, object? value) => Add(key, Unwrap(value));

    public bool ContainsKey(string key) => _dictionary.ContainsKey(key);

    public ICollection<string> Keys => _dictionary.Keys;

    public bool Remove(string key) => _dictionary.Remove(key);

    public bool TryGetValue(string key, out object? value) => _dictionary.TryGetValue(key, out value);

    public ICollection<object?> Values => _dictionary.Values;

    public void Add(KeyValuePair<string, object?> item) => _dictionary.Add(item);

    public void Clear() => _dictionary.Clear();

    public bool Contains(KeyValuePair<string, object?> item) => _dictionary.Contains(item);

    public void CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex) => _dictionary.CopyTo(array, arrayIndex);

    public int Count => _dictionary.Count;

    public bool IsReadOnly => _dictionary.IsReadOnly;

    public bool Remove(KeyValuePair<string, object?> item) => _dictionary.Remove(item);

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() => _dictionary.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
