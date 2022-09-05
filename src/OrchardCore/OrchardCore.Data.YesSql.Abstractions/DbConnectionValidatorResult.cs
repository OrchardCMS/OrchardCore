namespace OrchardCore.Data;

public enum DbConnectionValidatorResult
{
    Unknown,
    NoProvider,
    UnsupportedProvider,
    InvalidConnection,
    DatabaseAndPrefixInUse,
    Success,
}
