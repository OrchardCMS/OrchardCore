namespace OrchardCore.Infrastructure.Security;

[Flags]
public enum RoleType
{
    Standard = 0,
    System = 1,
    Owner = 2,
}
