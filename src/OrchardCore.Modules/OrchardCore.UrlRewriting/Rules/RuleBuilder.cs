using OrchardCore.UrlRewriting.ViewModels;
using System.Text;

namespace OrchardCore.UrlRewriting.Rules;

public class ApacheRuleBuilder
{
    public static string FlagsFromViewModel(RewriteRuleViewModel viewModel)
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

        return sbFlags.ToString();
    }

    public static string FromViewModel(RewriteRuleViewModel viewModel, bool includeFlags = true)
    {
        var flags = FlagsFromViewModel(viewModel); 

        var sbRewrite = new StringBuilder();
        var replaceUrl = viewModel.RuleAction == RuleAction.Rewrite ? viewModel.RewriteAction.RewriteUrl : viewModel.RedirectAction.RedirectUrl;

        var flagsStr = flags.Length > 0 ? $"[{flags}]" : "";
        if (flagsStr.Length > 0 && includeFlags)
        {
            sbRewrite.Append($"RewriteRule \"{viewModel.Pattern}\" \"{replaceUrl}\" {flagsStr}");
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
