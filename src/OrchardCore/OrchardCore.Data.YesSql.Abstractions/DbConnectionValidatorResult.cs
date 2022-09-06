namespace OrchardCore.Data;

/// <summary>
/// 'Unknown' indicates that the connection string status is unknown or was not validated.
/// 'NoProvider' indicated that the database provider is missing.
/// 'DocumentTableNotFound' indicates that the connection string was valid, while the Document table does not exists.
/// 'DocumentFound' indicates that the connection string was valid, while the Document table exists.
/// 'ShellDescriptorDocumentFound' indicates that the connection string was valid and the document table exists with at least one shell-descriptor document.
/// 'InvalidConnection' unable to open a connection to the given connection string.
/// 'UnsupportedProvider' indicates invalid or unsupported database provider.
/// </summary>
public enum DbConnectionValidatorResult
{
    Unknown,
    NoProvider,
    DocumentTableNotFound,
    DocumentTableFound,
    ShellDescriptorDocumentFound,
    InvalidConnection,
    UnsupportedProvider
}
