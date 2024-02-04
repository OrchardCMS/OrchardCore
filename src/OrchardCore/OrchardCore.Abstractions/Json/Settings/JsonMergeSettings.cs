namespace System.Text.Json.Settings;

#nullable enable

/// <summary>
/// Specifies the settings used when merging JSON.
/// </summary>
public class JsonMergeSettings
{
    private MergeArrayHandling _mergeArrayHandling;
    private MergeNullValueHandling _mergeNullValueHandling;
    private StringComparison _propertyNameComparison;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonMergeSettings"/> class.
    /// </summary>
    public JsonMergeSettings()
    {
        _propertyNameComparison = StringComparison.Ordinal;
    }

    /// <summary>
    /// Gets or sets the method used when merging JSON arrays.
    /// </summary>
    /// <value>The method used when merging JSON arrays.</value>
    public MergeArrayHandling MergeArrayHandling
    {
        get => _mergeArrayHandling;
        set
        {
            if (value < MergeArrayHandling.Concat || value > MergeArrayHandling.Merge)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            _mergeArrayHandling = value;
        }
    }

    /// <summary>
    /// Gets or sets how null value properties are merged.
    /// </summary>
    public MergeNullValueHandling MergeNullValueHandling
    {
        get => _mergeNullValueHandling;
        set
        {
            if (value < MergeNullValueHandling.Ignore || value > MergeNullValueHandling.Merge)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            _mergeNullValueHandling = value;
        }
    }

    /// <summary>
    /// Gets or sets the comparison used to match property names while merging.
    /// The exact property name will be searched for first and if no matching property is found then
    /// the <see cref="StringComparison"/> will be used to match a property.
    /// </summary>
    public StringComparison PropertyNameComparison
    {
        get => _propertyNameComparison;
        set
        {
            if (value < StringComparison.CurrentCulture || value > StringComparison.OrdinalIgnoreCase)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            _propertyNameComparison = value;
        }
    }
}
