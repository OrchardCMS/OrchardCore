using System;

namespace OrchardCore.Localization.Data;

public class DataLocalizedString
{
    public DataLocalizedString(string name, string context, string value)
        : this(name, context, value, resourceNotFound: false)
    {

    }

    public DataLocalizedString(string name, string context, string value, bool resourceNotFound)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(name, nameof(name));
        ArgumentNullException.ThrowIfNullOrEmpty(context, nameof(context));
        ArgumentNullException.ThrowIfNullOrEmpty(value, nameof(value));

        Name = name;
        Context = context;
        Value = value;
        ResourceNotFound = resourceNotFound;
    }

    public static implicit operator string(DataLocalizedString dataLocalizedString) => dataLocalizedString?.Value;

    public string Name { get; }

    public string Context { get; }

    public string Value { get; }

    public bool ResourceNotFound { get; }

    public override string ToString() => $"{Context}.{Name}";
}
