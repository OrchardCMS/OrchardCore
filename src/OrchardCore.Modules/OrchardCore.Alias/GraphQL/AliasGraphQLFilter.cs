using System.Linq.Expressions;
using Newtonsoft.Json.Linq;
using OrchardCore.Alias.Indexes;
using OrchardCore.Alias.Models;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.ContentManagement;
using YesSql;

namespace OrchardCore.Alias.GraphQL
{
    public class AliasWhereFilter : IWhereFilter<ContentItem>
    {
        private static readonly ParameterExpression AliasPartIndexParameter = Expression.Parameter(typeof(AliasPartIndex), "x");

        public void OnBeforeQuery(IQuery<ContentItem> query)
        {
            query.With<AliasPartIndex>();
        }

        public bool TryGetPropertyComparison(JProperty property, out Expression left, out Expression right)
        {
            if (property != null)
            {
                if (property.Name == "aliasPart")
                {
                    var aliasPart = property.Value.ToObject<AliasPart>();
                    if (!string.IsNullOrWhiteSpace(aliasPart?.Alias))
                    {
                        var aliasProperty = typeof(AliasPartIndex).GetProperty(nameof(AliasPartIndex.Alias));
                        
                        right = Expression.Constant(aliasPart.Alias);
                        left = Expression.Property(AliasPartIndexParameter, aliasProperty);

                        return true;
                    }
                }
            }

            left = null;
            right = null;
            return false;
        }
    }
}
