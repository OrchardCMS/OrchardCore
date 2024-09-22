using OrchardCore.Infrastructure.Security;

namespace OrchardCore.Security;

public interface IRole
{
    string RoleName { get; }

    string RoleDescription { get; }

    bool HasFullAccess { get; }

    public RoleType Type { get; set; }
}
