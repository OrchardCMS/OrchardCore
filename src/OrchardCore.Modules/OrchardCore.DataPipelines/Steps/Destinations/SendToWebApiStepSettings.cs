namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// The settings of a <see cref="SendToWebApiStep"/>.
/// </summary>
public sealed class SendToWebApiStepSettings
{
    /// <summary>
    /// The number of seconds to wait for each answer, by default.
    /// </summary>
    public const int DefaultTimeoutSeconds = 100;

    /// <summary>
    /// Gets or sets the absolute http or https URL the files are sent to.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// Gets or sets the HTTP method of the requests.
    /// </summary>
    public DataPipelineHttpMethod Method { get; set; } = DataPipelineHttpMethod.Post;

    /// <summary>
    /// Gets or sets how each file is sent in the body of its request.
    /// </summary>
    public DataPipelineHttpBodyMode BodyMode { get; set; } = DataPipelineHttpBodyMode.Multipart;

    /// <summary>
    /// Gets or sets the name of the form field that holds the file, when the body is a multipart form.
    /// </summary>
    public string FormFieldName { get; set; } = "file";

    /// <summary>
    /// Gets or sets the headers added to each request. They are not secret; set the credentials with
    /// <see cref="Authentication"/>.
    /// </summary>
    public List<DataPipelineHttpHeader> Headers { get; set; } = [];

    /// <summary>
    /// Gets or sets how the requests are authenticated.
    /// </summary>
    public DataPipelineHttpAuthentication Authentication { get; set; }

    /// <summary>
    /// Gets or sets the user name of the basic authentication.
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    /// Gets or sets the password of the basic authentication, protected with <see cref="Services.DataPipelineSecrets"/>.
    /// </summary>
    public string ProtectedPassword { get; set; }

    /// <summary>
    /// Gets or sets the bearer token, protected with <see cref="Services.DataPipelineSecrets"/>.
    /// </summary>
    public string ProtectedToken { get; set; }

    /// <summary>
    /// Gets or sets the name of the header that holds the API key.
    /// </summary>
    public string ApiKeyHeaderName { get; set; } = "X-API-Key";

    /// <summary>
    /// Gets or sets the API key, protected with <see cref="Services.DataPipelineSecrets"/>.
    /// </summary>
    public string ProtectedApiKey { get; set; }

    /// <summary>
    /// Gets or sets the number of seconds to wait for the answer to each request.
    /// </summary>
    public int TimeoutSeconds { get; set; } = DefaultTimeoutSeconds;
}

/// <summary>
/// A header added to the requests of a <see cref="SendToWebApiStep"/>.
/// </summary>
public sealed class DataPipelineHttpHeader
{
    /// <summary>
    /// Gets or sets the name of the header.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the value of the header.
    /// </summary>
    public string Value { get; set; }
}

/// <summary>
/// The HTTP method a <see cref="SendToWebApiStep"/> sends files with.
/// </summary>
public enum DataPipelineHttpMethod
{
    /// <summary>
    /// <c>POST</c>, usually to create a resource from each file.
    /// </summary>
    Post,

    /// <summary>
    /// <c>PUT</c>, usually to replace the resource at the URL.
    /// </summary>
    Put,
}

/// <summary>
/// How a <see cref="SendToWebApiStep"/> sends each file in the body of its request.
/// </summary>
public enum DataPipelineHttpBodyMode
{
    /// <summary>
    /// A <c>multipart/form-data</c> form with one field that holds the file, like an upload form of a web page.
    /// </summary>
    Multipart,

    /// <summary>
    /// The content of the file, with its media type as the <c>Content-Type</c> of the request.
    /// </summary>
    Raw,
}

/// <summary>
/// How a <see cref="SendToWebApiStep"/> authenticates its requests.
/// </summary>
public enum DataPipelineHttpAuthentication
{
    /// <summary>
    /// The requests are not authenticated.
    /// </summary>
    None,

    /// <summary>
    /// The <c>Authorization</c> header holds a user name and a password (basic authentication).
    /// </summary>
    Basic,

    /// <summary>
    /// The <c>Authorization</c> header holds a bearer token.
    /// </summary>
    Bearer,

    /// <summary>
    /// A header, such as <c>X-API-Key</c>, holds an API key.
    /// </summary>
    ApiKey,
}
