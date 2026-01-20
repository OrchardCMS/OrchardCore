using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Localization;
using OrchardCore.UrlRewriting.Models;

namespace OrchardCore.UrlRewriting;

public interface IUrlRewriteRuleSource
{
    /// <summary>
    /// Gets the unique technical name of the rule source.
    /// <para>
    /// This name is used to identify the source of the URL rewrite rules. 
    /// It should be unique across different sources to avoid conflicts.
    /// </para>
    /// </summary>
    string TechnicalName { get; }

    /// <summary>
    /// Gets a localized display name for the rule source.
    /// </summary>
    LocalizedString DisplayName { get; }

    /// <summary>
    /// Gets a localized description for the rule source.
    /// <para>
    /// This description provides more information about the source and its purpose.
    /// It is intended for display in user interfaces where users can select or configure 
    /// URL rewrite rules.
    /// </para>
    /// </summary>
    LocalizedString Description { get; }

    /// <summary>
    /// Configures the rewrite options for a specific URL rewrite rule.
    /// <para>
    /// This method is called to apply the settings and configurations specific to the 
    /// URL rewrite rule defined by the <paramref name="rule"/> parameter. 
    /// The <paramref name="options"/> parameter can be used to set various rewrite options, 
    /// such as the URL patterns to match or the actions to take on matches.
    /// </para>
    /// </summary>
    /// <param name="options">The rewrite options to configure.</param>
    /// <param name="rule">The specific URL rewrite rule to configure.</param>
    void Configure(RewriteOptions options, RewriteRule rule);
}
