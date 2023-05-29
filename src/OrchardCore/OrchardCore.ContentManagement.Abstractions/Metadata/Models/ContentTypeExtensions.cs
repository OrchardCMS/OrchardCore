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
    {
        return !String.IsNullOrEmpty(contentTypeDefinition.GetStereotype());
    }

    public static bool StereotypeEquals(this ContentTypeDefinition contentTypeDefinition, string stereotype)
    {
        return contentTypeDefinition.StereotypeEquals(stereotype, StringComparison.Ordinal);
    }

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
    {
        return contentTypeDefinition.GetSettings().Stereotype;
    }

    public static bool IsListable(this ContentTypeDefinition contentTypeDefinition)
    {
        return contentTypeDefinition.GetSettings().Listable;
    }

    public static bool IsCreatable(this ContentTypeDefinition contentTypeDefinition)
    {
        return contentTypeDefinition.GetSettings().Creatable;
    }

    public static bool IsDraftable(this ContentTypeDefinition contentTypeDefinition)
    {
        return contentTypeDefinition.GetSettings().Draftable;
    }

    public static bool IsVersionable(this ContentTypeDefinition contentTypeDefinition)
    {
        return contentTypeDefinition.GetSettings().Versionable;
    }

    public static bool IsSecurable(this ContentTypeDefinition contentTypeDefinition)
    {
        return contentTypeDefinition.GetSettings().Securable;
    }

    public static bool HasDescription(this ContentTypeDefinition contentTypeDefinition)
    {
        return !String.IsNullOrWhiteSpace(contentTypeDefinition.GetSettings().Description);
    }

    public static string GetDescription(this ContentTypeDefinition contentTypeDefinition)
    {
        return contentTypeDefinition.GetSettings().Description;
    }

    public static ContentTypeSettings GetSettings(this ContentTypeDefinition contentTypeDefinition)
    {
        return contentTypeDefinition.GetSettings<ContentTypeSettings>();
    }
}
