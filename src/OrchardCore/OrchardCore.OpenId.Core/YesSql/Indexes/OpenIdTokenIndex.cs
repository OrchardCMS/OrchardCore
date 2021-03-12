using System;
using OrchardCore.OpenId.YesSql.Models;
using YesSql.Indexes;

namespace OrchardCore.OpenId.YesSql.Indexes
{
    public class OpenIdTokenIndex : MapIndex
    {
        public string TokenId { get; set; }
        public string ApplicationId { get; set; }
        public string AuthorizationId { get; set; }
        public DateTime? CreationDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string ReferenceId { get; set; }
        public string Status { get; set; }
        public string Subject { get; set; }
        public string Type { get; set; }
    }

    public class OpenIdTokenIndexProvider : IndexProvider<OpenIdToken>
    {
        private const string OpenIdCollection = OpenIdToken.OpenIdCollection;

        public OpenIdTokenIndexProvider()
            => CollectionName = OpenIdCollection;

        public override void Describe(DescribeContext<OpenIdToken> context)
        {
            context.For<OpenIdTokenIndex>()
                .Map(token => new OpenIdTokenIndex
                {
                    TokenId = token.TokenId,
                    ApplicationId = token.ApplicationId,
                    AuthorizationId = token.AuthorizationId,
                    CreationDate = token.CreationDate,
                    ExpirationDate = token.ExpirationDate,
                    ReferenceId = token.ReferenceId,
                    Status = token.Status,
                    Subject = token.Subject,
                    Type = token.Type
                });
        }
    }
}
