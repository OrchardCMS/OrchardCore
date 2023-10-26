using System.Collections.Generic;
using System.Linq;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore;

public static class CssOrchardHelper
{
    public static string GetPartWrapperCssClasses(this IOrchardHelper helper, ContentTypePartDefinition partDefinition, bool skipThemeWrapperClasses = false)
    {
        var partClasses = GetPartWrapperCssClasses(partDefinition).ToArray();

        return skipThemeWrapperClasses ? string.Join(' ', partClasses) : helper.GetWrapperCssClasses(partClasses);
    }

    public static string GetFieldWrapperCssClasses(this IOrchardHelper helper, ContentPartFieldDefinition fieldDefinition)
    {
        return helper.GetWrapperCssClasses(GetFieldWrapperCssClasses(fieldDefinition).ToArray());
    }

    private static IEnumerable<string> GetPartWrapperCssClasses(ContentTypePartDefinition partDefinition)
    {
        yield return "content-part-wrapper";
        yield return $"content-part-wrapper-{partDefinition.PartDefinition.Name.HtmlClassify()}";

        if (partDefinition.IsNamedPart())
        {
            yield return $"content-part-wrapper-{partDefinition.Name.HtmlClassify()}";
        }
    }

    private static IEnumerable<string> GetFieldWrapperCssClasses(ContentPartFieldDefinition fieldDefinition)
    {
        yield return "field-wrapper";
        yield return $"field-wrapper-{fieldDefinition.GetReusableFieldWrapperClassName().HtmlClassify()}";

        if (fieldDefinition.IsNamedPart())
        {
            yield return $"field-wrapper-{fieldDefinition.GetFieldWrapperClassName().HtmlClassify()}";
        }
    }
}
