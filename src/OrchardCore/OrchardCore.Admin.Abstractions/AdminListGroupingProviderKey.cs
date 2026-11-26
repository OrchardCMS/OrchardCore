namespace OrchardCore.Admin;

/// <summary>
/// The DI key isolating grouping providers from other list extension points.
/// </summary>
/// <param name="ListName">The case-sensitive list name.</param>
public readonly record struct AdminListGroupingProviderKey(string ListName);
