namespace OrchardCore.Data;

public enum DbConnectionValidatorResult
{
    /// <summary>
    /// 'Unknown' indicates that the connection string status is unknown or was not validated.
    /// </summary>
    Unknown,

    /// <summary>
    /// 'NoProvider' indicated that the database provider is missing.
    /// </summary>
    NoProvider,

    /// <summary>
    /// 'DocumentTableNotFound' indicates that the connection string was valid, while the Document table does not exists.
    /// </summary>
    DocumentTableNotFound,

    /// <summary>
    /// 'DocumentTableFound' indicates that the connection string was valid, while the Document table exists.
    /// </summary>
    DocumentTableFound,

    /// <summary>
    /// 'ShellDescriptorDocumentFound' indicates that the connection string was valid and the document table exists with at least one shell-descriptor document.
    /// This also means that the Document table is used by a tenant.
    /// </summary>
    ShellDescriptorDocumentFound,

    /// <summary>
    /// 'InvalidConnection' unable to open a connection to the given connection string.
    /// </summary>
    InvalidConnection,

    /// <summary>
    /// 'UnsupportedProvider' indicates invalid or unsupported database provider.
    /// </summary>
    UnsupportedProvider
}
