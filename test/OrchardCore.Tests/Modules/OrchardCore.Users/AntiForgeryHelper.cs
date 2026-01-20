using System.Text.RegularExpressions;

namespace OrchardCore.Tests.OrchardCore.Users;

public partial class AntiForgeryHelper
{
    public static string ExtractAntiForgeryToken(string htmlResponseText)
    {
        ArgumentException.ThrowIfNullOrEmpty(htmlResponseText);

        var match = RequestVerificationTokenRegex().Match(htmlResponseText);

        return match.Success ? match.Groups[1].Captures[0].Value : null;
    }

    public static async Task<string> ExtractAntiForgeryToken(HttpResponseMessage response)
    {
        ArgumentNullException.ThrowIfNull(response);

        var raw = await response.Content.ReadAsStringAsync();

        return await Task.FromResult(ExtractAntiForgeryToken(raw));
    }

    [GeneratedRegex(@"\<input name=""__RequestVerificationToken"" type=""hidden"" value=""([^""]+)"" \/\>")]
    private static partial Regex RequestVerificationTokenRegex();
}
