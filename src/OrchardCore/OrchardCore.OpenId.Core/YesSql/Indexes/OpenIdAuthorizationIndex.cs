using System;
using OrchardCore.OpenId.YesSql.Models;
using YesSql.Indexes;

namespace OrchardCore.OpenId.YesSql.Indexes
{
    public class OpenIdAuthorizationIndex : MapIndex
    {
        public string AuthorizationId { get; set; }
        public string ApplicationId { get; set; }
        public DateTime? CreationDate { get; set; }
        public string Status { get; set; }
        public string Subject { get; set; }
        public string Type { get; set; }
    }

    public class OpenIdAuthorizationIndexProvider : IndexProvider<OpenIdAuthorization>
    {
        private const string OpenIdCollection = OpenIdAuthorization.OpenIdCollection;

        public OpenIdAuthorizationIndexProvider()
            => CollectionName = OpenIdCollection;

        public override void Describe(DescribeContext<OpenIdAuthorization> context)
        {
            context.For<OpenIdAuthorizationIndex>()
                .Map(authorization => new OpenIdAuthorizationIndex
                {
                    ApplicationId = authorization.ApplicationId,
                    AuthorizationId = authorization.AuthorizationId,
                    CreationDate = authorization.CreationDate,
                    Status = authorization.Status,
                    Subject = authorization.Subject,
                    Type = authorization.Type
                });
        }
    }
}
