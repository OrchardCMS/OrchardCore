using System;
using OrchardCore.ContentManagement.Metadata.Settings;

namespace OrchardCore.ContentManagement.Metadata.Models;

public static class ContentTypeExtensions
{
    public static bool TryGetStereotype(this ContentTypeDefinition contentTypeDefinition, out string stereotype)
    {
        stereotype = contentTypeDefinition.GetStereotype();

        return !String.IsNullOrWhiteSpace(stereotype);
    }

    public static bool HasStereotype(this ContentTypeDefinition contentTypeDefinition)
        => !String.IsNullOrEmpty(contentTypeDefinition.GetStereotype());

    public static bool StereotypeEquals(this ContentTypeDefinition contentTypeDefinition, string stereotype)
        => contentTypeDefinition.StereotypeEquals(stereotype, StringComparison.Ordinal);

    public static bool StereotypeEquals(this ContentTypeDefinition contentTypeDefinition, string stereotype, StringComparison stringComparison)
    {
        if (String.IsNullOrEmpty(stereotype))
        {
            throw new ArgumentNullException(nameof(stereotype));
        }

        return contentTypeDefinition.TryGetStereotype(out var st)
            && String.Equals(st, stereotype, stringComparison);
    }

    public static string GetStereotype(this ContentTypeDefinition contentTypeDefinition)
        => contentTypeDefinition.GetSettings().Stereotype;

    public static bool IsListable(this ContentTypeDefinition contentTypeDefinition)
        => contentTypeDefinition.GetSettings().Listable;

    public static bool IsCreatable(this ContentTypeDefinition contentTypeDefinition)
        => contentTypeDefinition.GetSettings().Creatable;

    public static bool IsDraftable(this ContentTypeDefinition contentTypeDefinition)
        => contentTypeDefinition.GetSettings().Draftable;

    public static bool IsVersionable(this ContentTypeDefinition contentTypeDefinition)
        => contentTypeDefinition.GetSettings().Versionable;

    public static bool IsSecurable(this ContentTypeDefinition contentTypeDefinition)
        => contentTypeDefinition.GetSettings().Securable;

    public static bool HasDescription(this ContentTypeDefinition contentTypeDefinition)
        => !String.IsNullOrWhiteSpace(contentTypeDefinition.GetSettings().Description);

    public static string GetDescription(this ContentTypeDefinition contentTypeDefinition)
        => contentTypeDefinition.GetSettings().Description;

    public static ContentTypeSettings GetSettings(this ContentTypeDefinition contentTypeDefinition)
        => contentTypeDefinition.GetSettings<ContentTypeSettings>();
}
