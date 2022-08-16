using System;
using OrchardCore.ContentManagement.Metadata.Settings;

namespace OrchardCore.ContentManagement.Metadata.Models;

public static class ContentTypeExtensions
{
    public static bool HasStereotype(this ContentTypeDefinition contentTypeDefinition)
    {
        return !String.IsNullOrEmpty(contentTypeDefinition.GetStereotypeOrDefault());
    }

    public static string GetStereotypeOrDefault(this ContentTypeDefinition contentTypeDefinition)
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
        return !String.IsNullOrEmpty(contentTypeDefinition.GetSettings().Description);
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
