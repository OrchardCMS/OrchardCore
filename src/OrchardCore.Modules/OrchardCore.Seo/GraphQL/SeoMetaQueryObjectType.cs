using GraphQL.Types;
using Microsoft.Extensions.Localization;
using OrchardCore.Media.Fields;
using OrchardCore.Seo.Models;

namespace OrchardCore.Seo.GraphQL;

public class SeoMetaQueryObjectType : ObjectGraphType<SeoMetaPart>
{
    public SeoMetaQueryObjectType(IStringLocalizer<SeoMetaQueryObjectType> S)
    {
        Name = nameof(SeoMetaPart);
        Description = S["SEO meta fields"];

        Field(x => x.Render)
            .Description(S["Whether to render the seo metas"]);

        Field(x => x.PageTitle, true)
            .Description(S["The seo page title"]);

        Field(x => x.MetaDescription, true)
            .Description(S["The meta description of the content item"]);

        Field(x => x.MetaKeywords, true)
            .Description(S["The meta keywords of the content item"]);

        Field(x => x.Canonical, true)
            .Description(S["The canonical link of the content item"]);

        Field(x => x.MetaRobots, true)
            .Description(S["The content item specific meta robots definition"]);

        Field<ListGraphType<MetaEntryQueryObjectType>>("customMetaTags")
            .Resolve(ctx => ctx.Source.CustomMetaTags);

        Field<ObjectGraphType<MediaField>>("defaultSocialImage")
            .Resolve(ctx => ctx.Source.DefaultSocialImage);

        Field<ObjectGraphType<MediaField>>("openGraphImage")
            .Resolve(ctx => ctx.Source.OpenGraphImage);

        Field(x => x.OpenGraphType, true)
            .Description(S["The seo meta opengraph type"]);

        Field(x => x.OpenGraphTitle, true)
            .Description(S["The seo meta opengraph title"]);

        Field(x => x.OpenGraphDescription, true)
            .Description(S["The seo meta opengraph description"]);

        Field<ObjectGraphType<MediaField>>("twitterImage")
            .Resolve(ctx => ctx.Source.TwitterImage);

        Field(x => x.TwitterTitle, true)
            .Description(S["The seo meta twitter title"]);

        Field(x => x.TwitterDescription, true)
            .Description(S["The seo meta twitter description"]);

        Field(x => x.TwitterCard, true)
            .Description(S["The seo meta twitter card"]);

        Field(x => x.TwitterCreator, true)
            .Description(S["The seo meta twitter creator"]);

        Field(x => x.TwitterSite, true)
            .Description(S["The seo meta twitter site"]);

        Field(x => x.GoogleSchema, true)
            .Description(S["The seo meta google schema"]);
    }
}
