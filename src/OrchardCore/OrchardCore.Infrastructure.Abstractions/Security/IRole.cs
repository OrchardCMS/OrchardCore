using OrchardCore.Infrastructure.Security;

namespace OrchardCore.Security;

public interface IRole
{
    string RoleName { get; }

    string RoleDescription { get; }

    RoleType Type { get; set; }
}
