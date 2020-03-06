using GraphQL.Types;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement.GraphQL.Options;
using OrchardCore.Settings;

namespace OrchardCore.ContentManagement.GraphQL.Queries
{
    public class SiteGraphType : ObjectGraphType<ISite>
    {
        private readonly GraphQLContentOptions _options;

        public SiteGraphType(IOptions<GraphQLContentOptions> optionsAccessor) {
            _options = optionsAccessor.Value;

            Name = "Site";

            Field(si => si.SiteName);
            Field(si => si.PageTitleFormat);

            // these seem bad to expose
            // Field(si => si.SiteSalt);
            // Field(si => si.SuperUser);

            Field(si => si.Calendar);
            Field(si => si.TimeZoneId);

            // IDK
            // Field(si => si.ResourceDebugMode);

            Field(si => si.UseCdn);
            Field(si => si.CdnBaseUrl);
            Field(si => si.PageSize);
            Field(si => si.MaxPageSize);
            Field(si => si.MaxPagedCount);
            Field(si => si.BaseUrl);

            // should not expose
            // Field(si => si.HomeRoute);

            Field(si => si.AppendVersion);

            IsTypeOf = IsContentType;
        }

        private bool IsContentType(object obj)
        {
            return obj is ISite;
        }
    }
}
