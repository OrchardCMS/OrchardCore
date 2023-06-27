using System;
using System.Collections.Generic;

namespace OrchardCore.Contents;

public class ContentSearchOption
{
    private readonly Dictionary<string, string> _maps = new(StringComparer.OrdinalIgnoreCase);

    public bool TryGetValue(string contentType, out string termName)
    {
        return _maps.TryGetValue(contentType, out termName);
    }

    public void AddTerm(string contentType, string termName)
    {
        ArgumentNullException.ThrowIfNull(contentType, nameof(contentType));

        _maps.TryAdd(contentType, termName);
    }

    public void RemoveTerm(string contentType)
    {
        ArgumentNullException.ThrowIfNull(contentType, nameof(contentType));

        if (_maps.ContainsKey(contentType))
        {
            _maps.Remove(contentType);
        }
    }
}
