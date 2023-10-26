using System.Collections.Generic;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore;

public static class CssOrchardHelper
{
    private const string FieldWrapperPrefix = "field-wrapper";
    private const string PartWrapperPrefix = "content-part-wrapper";
    public static string GetPartWrapperCssClasses(this IOrchardHelper helper, ContentTypePartDefinition partDefinition)
    {
        var items = new List<string>()
        {
            PartWrapperPrefix,
        };

        if (partDefinition != null)
        {
            items.Add($"{PartWrapperPrefix}-{partDefinition.PartDefinition.Name.HtmlClassify()}");
            if (partDefinition.IsNamedPart())
            {
                items.Add($"{PartWrapperPrefix}-{partDefinition.Name.HtmlClassify()}");
            }
        }

        return helper.GetWrapperCssClasses(items.ToArray());
    }
    public static string GetFieldWrapperCssClasses(this IOrchardHelper helper, ContentPartFieldDefinition fieldDefinition)
    {
        var items = new List<string>()
        {
            FieldWrapperPrefix
        };

        if (fieldDefinition != null)
        {
            items.Add($"{FieldWrapperPrefix}-{fieldDefinition.PartDefinition.Name}-{fieldDefinition.Name}".HtmlClassify());
            if (fieldDefinition.IsNamedPart())
            {
                items.Add($"{FieldWrapperPrefix}-{fieldDefinition.ContentTypePartDefinition.Name}-{fieldDefinition.Name}".HtmlClassify());
            }
        }

        return helper.GetWrapperCssClasses(items.ToArray());
    }
}
