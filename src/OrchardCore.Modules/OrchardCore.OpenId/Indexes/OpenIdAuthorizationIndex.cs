using OrchardCore.OpenId.Models;
using YesSql.Indexes;

namespace OrchardCore.OpenId.Indexes
{
    public class OpenIdAuthorizationIndex : MapIndex
    {
        public string AuthorizationId { get; set; }
        public string ApplicationId { get; set; }
        public string Status { get; set; }
        public string Subject { get; set; }
        public string Type { get; set; }
    }

    public class OpenIdAuthorizationIndexProvider : IndexProvider<OpenIdAuthorization>
    {
        public override void Describe(DescribeContext<OpenIdAuthorization> context)
        {
            context.For<OpenIdAuthorizationIndex>()
                .Map(authorization => new OpenIdAuthorizationIndex
                {
                    AuthorizationId = authorization.AuthorizationId,
                    ApplicationId = authorization.ApplicationId,
                    Status = authorization.Status,
                    Subject = authorization.Subject,
                    Type = authorization.Type
                });
        }
    }
}