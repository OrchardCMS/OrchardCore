namespace OrchardCore.Security;

public interface IRoleCreatedEventHandler
{
    Task RoleCreatedAsync(string roleName);
}
