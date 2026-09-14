using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Contents;
using OrchardCore.Navigation;

namespace OrchardCore.ContentTypes.Services;

/// <summary>
/// Describes the breadcrumb trails of the content types and content parts screens.
/// </summary>
public sealed class ContentTypesBreadcrumbProvider : IBreadcrumbProvider
{
    internal readonly IStringLocalizer S;

    public ContentTypesBreadcrumbProvider(IStringLocalizer<ContentTypesBreadcrumbProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public ValueTask BuildBreadcrumbAsync(BreadcrumbBuilder builder)
    {
        switch (builder.Name)
        {
            case ContentTypesBreadcrumbs.TypesList:
                AddTypes(builder);
                break;

            case ContentTypesBreadcrumbs.TypesCreate:
                AddTypes(builder);
                builder.Add(S["New Content Type"], item => item.Id("ContentType"));
                break;

            case ContentTypesBreadcrumbs.TypesEdit:
                AddTypes(builder);
                AddEditType(builder);
                break;

            case ContentTypesBreadcrumbs.TypesEditPart:
                AddTypes(builder);
                AddEditType(builder);
                builder.Add(S["Edit Part - {0}", builder.GetData<string>(ContentTypesBreadcrumbs.PartDisplayNameKey)],
                    item => item.Id("Part"));
                break;

            case ContentTypesBreadcrumbs.TypesAddParts:
                AddTypes(builder);
                AddEditType(builder);
                builder.Add(S["Add Parts"], item => item.Id("AddParts"));
                break;

            case ContentTypesBreadcrumbs.TypesAddReusablePart:
                AddTypes(builder);
                AddEditType(builder);
                builder.Add(S["Add Named Part"], item => item.Id("AddParts"));
                break;

            case ContentTypesBreadcrumbs.TypesAddField:
                AddTypes(builder);
                builder.Add(S["Add New Field To \"{0}\"", builder.GetData<string>(ContentTypesBreadcrumbs.PartDisplayNameKey)],
                    item => item.Id("AddField"));
                break;

            case ContentTypesBreadcrumbs.PartsList:
                AddParts(builder);
                break;

            case ContentTypesBreadcrumbs.PartsCreate:
                AddParts(builder);
                builder.Add(S["New Content Part"], item => item.Id("ContentPart"));
                break;

            case ContentTypesBreadcrumbs.PartsEdit:
                AddParts(builder);
                AddEditPart(builder);
                break;

            case ContentTypesBreadcrumbs.PartsEditField:
                AddParts(builder);
                AddEditPart(builder);
                builder.Add(S["\"{0}\" settings", builder.GetData<string>(ContentTypesBreadcrumbs.FieldDisplayNameKey)],
                    item => item.Id("Field"));
                break;
        }

        return ValueTask.CompletedTask;
    }

    private void AddTypes(BreadcrumbBuilder builder)
        => builder.Add(S["Content Types"], item => item
            .Id("ContentTypes")
            .Action("List", "Admin", new RouteValueDictionary { { "area", "OrchardCore.ContentTypes" } })
            .Permission(ContentTypesPermissions.ViewContentTypes));

    private void AddParts(BreadcrumbBuilder builder)
        => builder.Add(S["Content Parts"], item => item
            .Id("ContentParts")
            .Action("ListParts", "Admin", new RouteValueDictionary { { "area", "OrchardCore.ContentTypes" } })
            .Permission(ContentTypesPermissions.ViewContentTypes));

    private void AddEditType(BreadcrumbBuilder builder)
    {
        var typeName = builder.GetData<string>(ContentTypesBreadcrumbs.TypeNameKey);
        var typeDisplayName = builder.GetData<string>(ContentTypesBreadcrumbs.TypeDisplayNameKey);

        builder.Add(S["Edit Content Type - {0}", typeDisplayName], item => item
            .Id("ContentType")
            .Action("Edit", "Admin", new RouteValueDictionary
            {
                { "area", "OrchardCore.ContentTypes" },
                { "id", typeName },
            })
            .Permission(ContentTypesPermissions.EditContentTypes));
    }

    private void AddEditPart(BreadcrumbBuilder builder)
    {
        var partName = builder.GetData<string>(ContentTypesBreadcrumbs.PartNameKey);
        var partDisplayName = builder.GetData<string>(ContentTypesBreadcrumbs.PartDisplayNameKey);

        builder.Add(S["Edit Content Part - {0}", partDisplayName], item => item
            .Id("ContentPart")
            .Action("EditPart", "Admin", new RouteValueDictionary
            {
                { "area", "OrchardCore.ContentTypes" },
                { "id", partName },
            })
            .Permission(ContentTypesPermissions.EditContentTypes));
    }
}
