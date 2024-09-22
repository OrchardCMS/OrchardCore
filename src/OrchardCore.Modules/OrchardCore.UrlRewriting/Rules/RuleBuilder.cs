using OrchardCore.UrlRewriting.ViewModels;
using System.Text;

namespace OrchardCore.UrlRewriting.Rules;

public class ApacheRuleBuilder
{
    public static string FromViewModel(CreateUrlRewriteRuleViewModel viewModel, bool includeFlags = true)
    {
        var sbFlags = new StringBuilder();

        if (viewModel.IgnoreCase)
        {
            sbFlags.Append("NC,");
        };

        if (viewModel.RuleAction == RuleAction.Rewrite)
        {
            if (viewModel.RewriteAction.AppendQueryString)
            {
                sbFlags.Append("QSA,");
            }
            if (viewModel.RewriteAction.SkipFurtherRules == true)
            {
                sbFlags.Append("L,");
            }
        }

        if (viewModel.RuleAction == RuleAction.Redirect)
        {
            if (viewModel.RedirectAction.AppendQueryString)
            {
                sbFlags.Append("QSA,");
            }
            sbFlags.Append($"R={RedirectTypeToStatusCode(viewModel.RedirectAction.RedirectType)},");
        }

        if (sbFlags.Length > 0 && sbFlags[sbFlags.Length - 1] == ',')
        {
            sbFlags.Remove(sbFlags.Length - 1, 1);
        }

        var sbRewrite = new StringBuilder();
        var replaceUrl = viewModel.RuleAction == RuleAction.Rewrite ? viewModel.RewriteAction.RewriteUrl : viewModel.RedirectAction.RedirectUrl;

        var flags = sbFlags.Length > 0 ? $"[{sbFlags}]" : "";
        if (flags.Length > 0 && includeFlags)
        {
            sbRewrite.Append($"RewriteRule \"{viewModel.Pattern}\" \"{replaceUrl}\" {flags}");
        }
        else
        {
            sbRewrite.Append($"RewriteRule \"{viewModel.Pattern}\" \"{replaceUrl}\"");
        }

        return sbRewrite.ToString();
    }

    private static int RedirectTypeToStatusCode(RedirectType redirectType)
    {
        return redirectType switch
        {
            RedirectType.MovedPermanently301 => 301,
            RedirectType.Found302 => 302,
            RedirectType.TemporaryRedirect307 => 307,
            RedirectType.PernamentRedirect308 => 308,
            _ => 302,
        };
    }
}
