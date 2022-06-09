using System.Threading.Tasks;

namespace OrchardCore.Data;

public interface IConnectionValidator
{
    public Task<ConnectionValidatorResult> ValidateAsync(string providerName, string connectionString, string tablePrefix);
}

public enum ConnectionValidatorResult
{
    Unknown,
    ValidDocumentDoesNotExists,
    ValidDocumentExists,
    InvalidConnection
}
