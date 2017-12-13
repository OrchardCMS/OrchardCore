using OrchardCore.OpenId.Models;
using YesSql.Indexes;

namespace OrchardCore.OpenId.Indexes
{
    public class OpenIdAuthorizationIndex : MapIndex
    {
        public int? ApplicationId { get; set; }
        public string Status { get; set; }
        public string Subject { get; set; }
        public string Type { get; set; }
    }

    public class OpenIdAuthorizationIndexProvider : IndexProvider<OpenIdAuthorization>
    {
        public override void Describe(DescribeContext<OpenIdAuthorization> context)
        {
            context.For<OpenIdAuthorizationIndex>()
                .Map(application => new OpenIdAuthorizationIndex
                {
                    ApplicationId = application.ApplicationId,
                    Status = application.Status,
                    Subject = application.Subject,
                    Type = application.Type
                });
        }
    }
}