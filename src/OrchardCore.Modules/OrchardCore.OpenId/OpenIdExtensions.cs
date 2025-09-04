using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace OrchardCore.OpenId;

internal static class OpenIdExtensions
{
    internal static string GetUserIdentifier(this ClaimsIdentity identity)
        => identity.FindFirst(Claims.Subject)?.Value ??
           identity.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
           identity.FindFirst(ClaimTypes.Upn)?.Value ??
           throw new InvalidOperationException("No suitable user identifier can be found in the identity.");

    internal static string GetUserIdentifier(this ClaimsPrincipal principal)
        => principal.FindFirst(Claims.Subject)?.Value ??
           principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
           principal.FindFirst(ClaimTypes.Upn)?.Value ??
           throw new InvalidOperationException("No suitable user identifier can be found in the principal.");

    internal static string GetUserName(this ClaimsPrincipal principal)
        => principal.FindFirst(Claims.Name)?.Value ??
           principal.FindFirst(ClaimTypes.Name)?.Value ??
           throw new InvalidOperationException("No suitable user name can be found in the principal.");

    internal static string[] GetRoles(this ClaimsPrincipal principal)
        => principal.FindAll(c => c.Type is Claims.Role or ClaimTypes.Role)
        .Select(x => x.Value)
        .Distinct()
        .ToArray();

    internal static async Task<bool> AnyAsync<T>(this IAsyncEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        await using var enumerator = source.GetAsyncEnumerator();
        return await enumerator.MoveNextAsync();
    }

    internal static Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return ExecuteAsync();

        async Task<List<T>> ExecuteAsync()
        {
            var list = new List<T>();

            await foreach (var element in source)
            {
                list.Add(element);
            }

            return list;
        }
    }
}
