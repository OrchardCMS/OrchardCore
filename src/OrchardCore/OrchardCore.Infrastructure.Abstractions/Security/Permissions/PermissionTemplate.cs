namespace OrchardCore.Security.Permissions;

public sealed record PermissionTemplate(
    string Name,
    string Description,
    string Category,
    IEnumerable<Permission> ImpliedBy,
    IEnumerable<PermissionTemplate> ImpliedByTemplates,
    bool IsSecurityCritical = false)
{
    public PermissionTemplate(string name, string description, string category = null, params Permission[] impliedBy)
        : this(name, description, category, impliedBy, ImpliedByTemplates: [], IsSecurityCritical: false)
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
        [.. ImpliedBy, .. ImpliedByTemplates.Select(template => template.CreateDynamicPermission(nameValue, descriptionValue))],
        IsSecurityCritical)
    {
        Category = string.IsNullOrEmpty(Category) ? null : string.Format(Category, nameValue, descriptionValue),
    };
}
