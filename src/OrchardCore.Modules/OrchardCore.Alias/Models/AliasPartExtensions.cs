using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Alias.Indexes;
using YesSql;

namespace OrchardCore.Alias.Models
{
    public static class AliasPartExtensions
    {
        public static async IAsyncEnumerable<ValidationResult> ValidateAsync(this AliasPart part, IStringLocalizer S, ISession session)
        {
            if (!string.IsNullOrWhiteSpace(part.Alias))
            {
                if (part.Alias.Length > AliasPart.MaxAliasLength)
                {
                    yield return new ValidationResult(S["Your alias is too long. The alias can only be up to {0} characters. \"{1}\"", AliasPart.MaxAliasLength, part.Alias], new string[] { nameof(part.Alias) });
                }

                if (!await IsAliasUniqueAsync(part, session, part.Alias))
                {
                    yield return new ValidationResult(S["Your alias is already in use. \"{0}\"", part.Alias], new[] { nameof(part.Alias) });
                }
            }
        }

        public static async Task<bool> IsAliasUniqueAsync(this AliasPart context, ISession session, string alias)
        {
            return (await session.QueryIndex<AliasPartIndex>(o => o.Alias == alias && o.ContentItemId != context.ContentItem.ContentItemId).CountAsync()) == 0;
        }

    }
}
