using OrchardCore.Infrastructure.Security;

namespace OrchardCore.Security;

public interface IRole
{
    string RoleName { get; }

    string RoleDescription { get; }

    bool HasFullAccess { get; }

    RoleType Type { get; set; }
}
