namespace OrchardCore.Admin;

/// <summary>
/// Identifies the keyed column providers registered for one admin list.
/// </summary>
/// <param name="ListName">The case-sensitive list name declared by the rendering view.</param>
public readonly record struct AdminListColumnsProviderKey(string ListName);
