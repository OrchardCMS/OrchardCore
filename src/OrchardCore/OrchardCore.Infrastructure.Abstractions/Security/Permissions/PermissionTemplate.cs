using OrchardCore.Localization;

namespace OrchardCore.Security.Permissions;

public sealed record PermissionTemplate
{
    public string Name { get; init; }
    public LocalizationSource Description { get; init; }
    public string Category { get; init; }
    public IEnumerable<Permission> ImpliedBy { get; init; } = [];
    public IEnumerable<PermissionTemplate> ImpliedByTemplates { get; init; } = [];
    public bool IsSecurityCritical { get; init; }
    
    public PermissionTemplate(string name, LocalizationSource description, string category = null, params Permission[] impliedBy)
    {
        Name = name;
        Description = description;
        Category = category;
        ImpliedBy = impliedBy ?? [];
    }
    
    public PermissionTemplate(string name, string description = null, string category = null, params Permission[] impliedBy)
        : this(name, description is null ? null : LocalizationSource.Create(description), category, impliedBy)
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
        string.Format(Description.Value, descriptionValue ?? nameValue),
        [.. ImpliedBy, .. ImpliedByTemplates.Select(template => template.CreateDynamicPermission(nameValue, descriptionValue))],
        IsSecurityCritical)
    {
        Category = string.IsNullOrEmpty(Category) ? null : string.Format(Category, nameValue, descriptionValue),
    };
}
