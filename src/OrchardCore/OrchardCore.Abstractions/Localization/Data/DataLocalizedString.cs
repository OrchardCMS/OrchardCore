namespace OrchardCore.Localization.Data;

public class DataLocalizedString
{
    public DataLocalizedString(string context, string name, string value)
        : this(context, name, value, resourceNotFound: false)
    {

    }

    public DataLocalizedString(string context, string name, string value, bool resourceNotFound)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(context, nameof(context));
        ArgumentNullException.ThrowIfNullOrEmpty(name, nameof(name));
        //ArgumentNullException.ThrowIfNullOrEmpty(value, nameof(value));

        Context = context;
        Name = name;
        Value = value;
        ResourceNotFound = resourceNotFound;
    }

    public static implicit operator string(DataLocalizedString dataLocalizedString) => dataLocalizedString?.Value;

    public string Context { get; }

    public string Name { get; }

    public string Value { get; }

    public bool ResourceNotFound { get; }

    public override string ToString() => $"{Context}.{Name}";
}
