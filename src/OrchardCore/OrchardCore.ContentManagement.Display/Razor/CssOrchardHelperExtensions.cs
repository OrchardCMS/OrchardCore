namespace OrchardCore;

public static class CssOrchardHelperExtensions
{
    private const string FieldWrapperPrefix = "field-wrapper";
    private const string PartWrapperPrefix = "content-part-wrapper";

    public static string GetPartWrapperCssClasses(this IOrchardHelper helper, ContentTypePartDefinition partDefinition, params string[] additionalClasses)
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

        if (additionalClasses?.Length > 0)
        {
            items.AddRange(additionalClasses);
        }

        return helper.GetWrapperCssClasses(items.ToArray());
    }

    public static string GetFieldWrapperCssClasses(this IOrchardHelper helper, ContentPartFieldDefinition fieldDefinition, params string[] additionalClasses)
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

        if (additionalClasses?.Length > 0)
        {
            items.AddRange(additionalClasses);
        }

        return helper.GetWrapperCssClasses(items.ToArray());
    }
}
