using Microsoft.AspNetCore.Rewrite;
using OrchardCore.UrlRewriting.ViewModels;

namespace OrchardCore.UrlRewriting.Rules;

public sealed class ApacheRuleValidator
{
    public static bool ValidateRule(RewriteRuleViewModel viewModel, out string validationError)
    {
        var apacheRule = ApacheRules.FromViewModel(viewModel, true);

        try
        {
            var rewriteOptions = new RewriteOptions();
            using var apacheModRewrite = new StringReader(apacheRule);
            rewriteOptions.AddApacheModRewrite(apacheModRewrite);
        }
        catch (FormatException ex)
        {
            validationError = ex.Message;
            return false;
        }
        catch (Exception ex)
        {
            validationError = ex.Message;
            return false;
        }

        validationError = null;
        return true;
    }
}
