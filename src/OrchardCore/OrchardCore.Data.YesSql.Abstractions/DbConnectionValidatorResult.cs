namespace OrchardCore.Data;

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
    /// The connection string is valid. But, the Document table does not exists.
    /// </summary>
    DocumentTableNotFound,

    /// <summary>
    /// The connection string is valid and the Document table exists.
    /// </summary>
    DocumentTableFound,

    /// <summary>
    /// The connection string is valid and the Document table exists with at least one shell-descriptor document.
    /// This also means that the Document table is used by an existing tenant.
    /// </summary>
    ShellDescriptorDocumentFound,

    /// <summary>
    /// Unable to open a connection to the given database connection string.
    /// </summary>
    InvalidConnection,

    /// <summary>
    /// Invalid or unsupported database provider.
    /// </summary>
    UnsupportedProvider
}
