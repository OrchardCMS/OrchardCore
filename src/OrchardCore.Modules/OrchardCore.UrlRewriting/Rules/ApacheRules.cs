using OrchardCore.UrlRewriting.Models;
using OrchardCore.UrlRewriting.ViewModels;
using System.Text;

namespace OrchardCore.UrlRewriting.Rules;

public sealed class ApacheRules
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

    public static string FromModel(RewriteRule model)
    {
        if (string.IsNullOrWhiteSpace(model.Flags))
        {
            return $"RewriteRule \"{model.Pattern}\" \"{model.Substitution}\"";
        }
        else
        {
            return $"RewriteRule \"{model.Pattern}\" \"{model.Substitution}\" [{model.Flags}]";
        }
    }

    public static string FromModels(IEnumerable<RewriteRule> models)
    {
        var sb = new StringBuilder();
        foreach (var model in models)
        {
            sb.AppendLine(FromModel(model));
        }
        return sb.ToString();
    }

    public static RedirectType GetRedirectType(string flag)
    {
        return flag switch
        {
            "R=301" => RedirectType.MovedPermanently301,
            "R=302" => RedirectType.Found302,
            "R=307" => RedirectType.TemporaryRedirect307,
            "R=308" => RedirectType.PernamentRedirect308,
            _ => RedirectType.Found302
        };
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
