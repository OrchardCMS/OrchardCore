using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OrchardCore.Settings;

internal sealed class SiteSettingsResponse
{
    [JsonPropertyName("siteName")]
    public string SiteName { get; init; }

    [JsonPropertyName("pageTitleFormat")]
    public string PageTitleFormat { get; init; }

    [JsonPropertyName("baseUrl")]
    public string BaseUrl { get; init; }

    [JsonPropertyName("timeZoneId")]
    public string TimeZoneId { get; init; }

    [JsonPropertyName("pageSize")]
    public int PageSize { get; init; }

    [JsonPropertyName("maxPageSize")]
    public int MaxPageSize { get; init; }

    [JsonPropertyName("maxPagedCount")]
    public int MaxPagedCount { get; init; }

    [JsonPropertyName("calendar")]
    public string Calendar { get; init; }

    [JsonPropertyName("resourceDebugMode")]
    public string ResourceDebugMode { get; init; }

    [JsonPropertyName("useCdn")]
    public bool UseCdn { get; init; }

    [JsonPropertyName("cdnBaseUrl")]
    public string CdnBaseUrl { get; init; }

    [JsonPropertyName("appendVersion")]
    public bool AppendVersion { get; init; }

    [JsonPropertyName("cacheMode")]
    public string CacheMode { get; init; }
}

internal sealed class SiteSettingsManagementSchemaResponse
{
    [JsonPropertyName("core")]
    public JsonObject Core { get; init; }

    [JsonPropertyName("sections")]
    public SiteSettingsManagementSchemaSectionResponse[] Sections { get; init; } = [];
}

internal sealed class SiteSettingsManagementSchemaSectionResponse
{
    [JsonPropertyName("name")]
    public string Name { get; init; }

    [JsonPropertyName("displayName")]
    public string DisplayName { get; init; }

    [JsonPropertyName("description")]
    public string Description { get; init; }

    [JsonPropertyName("schema")]
    public JsonObject Schema { get; init; }

    [JsonPropertyName("readable")]
    public bool Readable { get; init; }

    [JsonPropertyName("writable")]
    public bool Writable { get; init; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed class SiteSettingsUpdateRequest
{
    private string _siteName;
    private string _pageTitleFormat;
    private string _baseUrl;
    private string _timeZoneId;
    private int _pageSize;
    private int _maxPageSize;
    private int _maxPagedCount;
    private string _calendar;
    private string _resourceDebugMode;
    private bool _useCdn;
    private string _cdnBaseUrl;
    private bool _appendVersion;
    private string _cacheMode;

    [JsonPropertyName("siteName")]
    public string SiteName
    {
        get => _siteName;
        set
        {
            _siteName = value;
            HasSiteName = true;
        }
    }

    [JsonPropertyName("pageTitleFormat")]
    public string PageTitleFormat
    {
        get => _pageTitleFormat;
        set
        {
            _pageTitleFormat = value;
            HasPageTitleFormat = true;
        }
    }

    [JsonPropertyName("baseUrl")]
    public string BaseUrl
    {
        get => _baseUrl;
        set
        {
            _baseUrl = value;
            HasBaseUrl = true;
        }
    }

    [JsonPropertyName("timeZoneId")]
    public string TimeZoneId
    {
        get => _timeZoneId;
        set
        {
            _timeZoneId = value;
            HasTimeZoneId = true;
        }
    }

    [JsonPropertyName("pageSize")]
    public int PageSize
    {
        get => _pageSize;
        set
        {
            _pageSize = value;
            HasPageSize = true;
        }
    }

    [JsonPropertyName("maxPageSize")]
    public int MaxPageSize
    {
        get => _maxPageSize;
        set
        {
            _maxPageSize = value;
            HasMaxPageSize = true;
        }
    }

    [JsonPropertyName("maxPagedCount")]
    public int MaxPagedCount
    {
        get => _maxPagedCount;
        set
        {
            _maxPagedCount = value;
            HasMaxPagedCount = true;
        }
    }

    [JsonPropertyName("calendar")]
    public string Calendar
    {
        get => _calendar;
        set
        {
            _calendar = value;
            HasCalendar = true;
        }
    }

    [JsonPropertyName("resourceDebugMode")]
    public string ResourceDebugMode
    {
        get => _resourceDebugMode;
        set
        {
            _resourceDebugMode = value;
            HasResourceDebugMode = true;
        }
    }

    [JsonPropertyName("useCdn")]
    public bool UseCdn
    {
        get => _useCdn;
        set
        {
            _useCdn = value;
            HasUseCdn = true;
        }
    }

    [JsonPropertyName("cdnBaseUrl")]
    public string CdnBaseUrl
    {
        get => _cdnBaseUrl;
        set
        {
            _cdnBaseUrl = value;
            HasCdnBaseUrl = true;
        }
    }

    [JsonPropertyName("appendVersion")]
    public bool AppendVersion
    {
        get => _appendVersion;
        set
        {
            _appendVersion = value;
            HasAppendVersion = true;
        }
    }

    [JsonPropertyName("cacheMode")]
    public string CacheMode
    {
        get => _cacheMode;
        set
        {
            _cacheMode = value;
            HasCacheMode = true;
        }
    }

    [JsonIgnore]
    internal bool HasSiteName { get; private set; }

    [JsonIgnore]
    internal bool HasPageTitleFormat { get; private set; }

    [JsonIgnore]
    internal bool HasBaseUrl { get; private set; }

    [JsonIgnore]
    internal bool HasTimeZoneId { get; private set; }

    [JsonIgnore]
    internal bool HasPageSize { get; private set; }

    [JsonIgnore]
    internal bool HasMaxPageSize { get; private set; }

    [JsonIgnore]
    internal bool HasMaxPagedCount { get; private set; }

    [JsonIgnore]
    internal bool HasCalendar { get; private set; }

    [JsonIgnore]
    internal bool HasResourceDebugMode { get; private set; }

    [JsonIgnore]
    internal bool HasUseCdn { get; private set; }

    [JsonIgnore]
    internal bool HasCdnBaseUrl { get; private set; }

    [JsonIgnore]
    internal bool HasAppendVersion { get; private set; }

    [JsonIgnore]
    internal bool HasCacheMode { get; private set; }
}
