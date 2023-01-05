namespace OrchardCore.Data;

/// <summary>
/// The result from validating a database connection using <see cref="IDbConnectionValidator"/>.
/// </summary>
public enum DbConnectionValidatorResult
{
    /// <summary>
    /// The connection string status is unknown or was not validated.
    /// </summary>
    Unknown,

    /// <summary>
    /// The database provider is missing.
    /// </summary>
    NoProvider,

    /// <summary>
    /// The connection string is valid and the 'Document' table does not exists.
    /// </summary>
    DocumentTableNotFound,

    /// <summary>
    /// The connection string is valid and the 'Document' table exists.
    /// </summary>
    DocumentTableFound,

    /// <summary>
    /// The 'Document' table exists with no 'ShellDescriptor' document.
    /// </summary>
    ShellDescriptorDocumentNotFound,

    /// <summary>
    /// Unable to open a connection with the given database connection string.
    /// </summary>
    InvalidConnection,

    /// <summary>
    /// Unsupported database provider.
    /// </summary>
    UnsupportedProvider
}
