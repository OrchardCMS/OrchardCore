using System.Threading.Tasks;

namespace OrchardCore.Data;

public interface IDbConnectionValidator
{
    Task<DbConnectionValidatorResult> ValidateAsync(DbConnectionValidatorContext context);
}
