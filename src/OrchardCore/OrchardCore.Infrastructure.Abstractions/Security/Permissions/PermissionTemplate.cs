namespace OrchardCore.Security.Permissions;

public record PermissionTemplate(
    string Name,
    string Description,
    string Category,
    IEnumerable<Permission> ImpliedBy,
    bool IsSecurityCritical = false)
{
    public PermissionTemplate(string name, string description, string category = null, params Permission[] impliedBy)
        : this(name, description, category, impliedBy, IsSecurityCritical: false)
    {
    }

    public PermissionTemplate(string name, string description, params Permission[] impliedBy)
        : this(name, description, category: null, impliedBy)
    {
    }

    public Permission CreateDynamicPermission(string nameValue) =>
        CreateDynamicPermission(nameValue, nameValue);

    public Permission CreateDynamicPermission(string nameValue, string descriptionValue) => new(
        string.Format(Name, nameValue),
        string.Format(Description, descriptionValue ?? nameValue),
        ImpliedBy ?? [],
        IsSecurityCritical)
    {
        Category = string.IsNullOrEmpty(Category) ? null : string.Format(Category, nameValue, descriptionValue),
    };
}
