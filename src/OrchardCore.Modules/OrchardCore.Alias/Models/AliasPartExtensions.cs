using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Localization;
using OrchardCore.Alias.Indexes;
using YesSql;

namespace OrchardCore.Alias.Models
{
    internal static class AliasPartExtensions
    {
        internal static async IAsyncEnumerable<ValidationResult> ValidateAsync(this AliasPart part, IStringLocalizer S, ISession session)
        {
            if (part.Alias.Length > AliasPart.MaxAliasLength)
            {
                yield return new ValidationResult(S["Your alias is too long. The alias can only be up to {0} characters.", AliasPart.MaxAliasLength], new string[] { nameof(part.Alias) });
            }

            if (!await IsAliasUniqueAsync(part, session, part.Alias))
            {
                yield return new ValidationResult(S["Your alias is already in use."], new[] { nameof(part.Alias) });
            }
        }

        internal static async Task<bool> IsAliasUniqueAsync(this AliasPart context, ISession session, string alias)
        {
            return (await session.QueryIndex<AliasPartIndex>(o => o.Alias == alias && o.ContentItemId != context.ContentItem.ContentItemId).CountAsync()) == 0;
        }

    }
}
