using System;
using OrchardCore.OpenId.Models;
using YesSql.Indexes;

namespace OrchardCore.OpenId.Indexes
{
    public class OpenIdTokenIndex : MapIndex
    {
        public int? ApplicationId { get; set; }
        public int? AuthorizationId { get; set; }
        public DateTimeOffset? ExpirationDate { get; set; }
        public string ReferenceId { get; set; }
        public string Status { get; set; }
        public string Subject { get; set; }
        public int TokenId { get; set; }
    }

    public class OpenIdTokenIndexProvider : IndexProvider<OpenIdToken>
    {
        public override void Describe(DescribeContext<OpenIdToken> context)
        {
            context.For<OpenIdTokenIndex>()
                .Map(token => new OpenIdTokenIndex
                {
                    ApplicationId = token.ApplicationId,
                    AuthorizationId = token.AuthorizationId,
                    ExpirationDate = token.ExpirationDate,
                    ReferenceId = token.ReferenceId,
                    Status = token.Status,
                    Subject = token.Subject,
                    TokenId = token.Id
                });
        }
    }
}