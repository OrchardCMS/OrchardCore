namespace OrchardCore.Data;

public enum DbConnectionValidatorResult
{
    // Unknown indicates that the connection string status is unknown or was not yet validated
    Unknown,

    // NoProvider indicated that the provider is missing
    NoProvider,

    // DocumentNotFound indicates that the connection string was valid, yet the Document table does not exist 
    DocumentNotFound,

    // DocumentFound indicates that the connection string was valid, yet the Document table exist
    DocumentFound,

    // InvalidConnection unable to open a connection to the given connection string
    InvalidConnection,

    // UnsupportedProvider indicated invalid or unsupported database provider
    UnsupportedProvider
}
