using OrchardCore.Localization;

namespace OrchardCore.UrlRewriting;

/// <summary>
/// Describes an <see cref="IUrlRewriteRuleSource"/> with the static members of its class, so the rule sources can be
/// listed without creating them, and are translated for the current culture when they are shown. Every rule source
/// registered with <c>services.AddRewriteRuleSource&lt;TSource&gt;(name)</c> implements it. It is separate from
/// <see cref="IUrlRewriteRuleSource"/>, because an interface with static abstract members can't be used as a type argument.
/// </summary>
public interface IUrlRewriteRuleSourceDescription
{
    /// <summary>
    /// Gets the display name of the rule source, to localize with <c>IStringLocalizerFactory.Localize</c>.
    /// </summary>
    static abstract LocalizationSource DisplayName { get; }

    /// <summary>
    /// Gets the description of the rule source, to localize with <c>IStringLocalizerFactory.Localize</c>. It provides
    /// more information about the source and its purpose, and is shown where users select the source of a rule.
    /// </summary>
    static abstract LocalizationSource Description { get; }
}
