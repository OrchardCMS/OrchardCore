namespace OrchardCore.Data;

public enum DbConnectionValidatorResult
{
    // 'Unknown' indicates that the connection string status is unknown or was not yet validated.
    Unknown,

    // 'NoProvider' indicated that the provider is missing.
    NoProvider,

    // 'DocumentTableNotFound' indicates that the connection string was valid, yet the Document table does not exist.
    DocumentTableNotFound,

    // 'DocumentFound' indicates that the connection string was valid, yet the Document table exist.
    DocumentTableFound,

    // 'ShellDescriptorDocumentFound' indicates that the connection string was valid and the document table exists with at least one entry of shell-descriptor document.
    ShellDescriptorDocumentFound,

    // 'InvalidConnection' unable to open a connection to the given connection string.
    InvalidConnection,

    // 'UnsupportedProvider' indicated invalid or unsupported database provider.
    UnsupportedProvider
}
