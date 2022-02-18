using GraphQL.Types;
using Microsoft.Extensions.Localization;
using OrchardCore.Media.Fields;
using OrchardCore.ResourceManagement;
using OrchardCore.Seo.Models;

namespace OrchardCore.Seo.GraphQL;

public class SeoMetaQueryObjectType : ObjectGraphType<SeoMetaPart>
{
    public SeoMetaQueryObjectType(IStringLocalizer<SeoMetaQueryObjectType> S)
    {
        Name = nameof(SeoMetaPart);
        Description = S["SEO meta fields"];

        Field(x => x.Render)
            .Description("Whether to render the seo metas");
        Field(x => x.PageTitle)
            .Description("The seo page title");
        Field(x => x.MetaDescription)
            .Description("The meta description of the content item");
        Field(x => x.MetaKeywords)
            .Description("The meta keywords of the content item");
        Field(x => x.Canonical)
            .Description("The canonical link of the content item");
        Field( x => x.MetaRobots)
            .Description("The content item specific meta robots definition");

        Field<ListGraphType<MetaEntryQueryObjectType>>()
            .Name("customMetaTags")
            .Resolve(ctx => ctx.Source.CustomMetaTags);

        Field<ObjectGraphType<MediaField>>()
            .Name("defaultSocialImage")
            .Resolve(ctx => ctx.Source.DefaultSocialImage);

        Field<ObjectGraphType<MediaField>>()
            .Name("openGraphImage")
            .Resolve(ctx => ctx.Source.OpenGraphImage);

        Field( x => x.OpenGraphType)
            .Description("The seo meta opengraph type");
        Field( x => x.OpenGraphTitle)
            .Description("The seo meta opengraph title");
        Field( x => x.OpenGraphDescription)
            .Description("The seo meta opengraph description");

        Field<ObjectGraphType<MediaField>>()
            .Name("twitterImage")
            .Resolve(ctx => ctx.Source.TwitterImage);

        Field( x => x.TwitterTitle)
            .Description("The seo meta twitter title");
        Field( x => x.TwitterDescription)
            .Description("The seo meta twitter description");
        Field( x => x.TwitterCard)
            .Description("The seo meta twitter card");
        Field( x => x.TwitterCreator)
            .Description("The seo meta twitter creator");
        Field( x => x.TwitterSite)
            .Description("The seo meta twitter site");

        Field( x => x.GoogleSchema)
            .Description("The seo meta google schema");

    }
}
