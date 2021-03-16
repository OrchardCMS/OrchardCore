namespace OrchardCore.Security
{
    public interface IRole
    {
        string RoleName { get; }

        string RoleDescription { get; }
    }
}
