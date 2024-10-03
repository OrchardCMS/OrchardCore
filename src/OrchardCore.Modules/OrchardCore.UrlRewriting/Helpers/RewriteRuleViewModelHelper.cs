using OrchardCore.UrlRewriting.Models;
using OrchardCore.UrlRewriting.Rules;
using OrchardCore.UrlRewriting.ViewModels;

namespace OrchardCore.UrlRewriting.Helpers;

internal static class RewriteRuleViewModelHelper
{
    public static void FromModel(this RewriteRuleViewModel viewModel, RewriteRule model)
    {
        var flags = model.GetFlagsCollection();

        var isRedirect = flags.Where(f => f.StartsWith("R=")).Any();

        viewModel.Id = model.Id;
        viewModel.Name = model.Name;
        viewModel.Pattern = model.Pattern;
        viewModel.IgnoreCase = flags.Contains("NC");
        viewModel.RuleAction = isRedirect ? RuleAction.Redirect : RuleAction.Rewrite;

        if (isRedirect)
        {
            var redirectFlag = flags.Where(f => f.StartsWith("R=")).FirstOrDefault() ?? string.Empty;

            viewModel.RedirectAction.RedirectUrl = model.Substitution;
            viewModel.RedirectAction.AppendQueryString = flags.Contains("QSA");
            viewModel.RedirectAction.RedirectType = ApacheRules.GetRedirectType(redirectFlag);
        }
        else
        {
            viewModel.RewriteAction.RewriteUrl = model.Substitution;
            viewModel.RewriteAction.AppendQueryString = flags.Contains("QSA");
            viewModel.RewriteAction.SkipFurtherRules = flags.Contains("L");
        }
    }
}
