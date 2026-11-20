namespace OrchardCore.DisplayManagement.Descriptors;

/// <summary>
/// The names of the tabs and cards that placement groups shapes into, as they are translated with data localization.
/// </summary>
/// <remarks>
/// A name is the identifier of its group, in placement rules and in the generated element ids, so only its caption is
/// translated, under the <see cref="DataLocalizationContext"/> context.
/// </remarks>
public static class TabAndCardNames
{
    /// <summary>
    /// The data localization context of the tab and card captions.
    /// </summary>
    public const string DataLocalizationContext = "Tabs and Cards";

    /// <summary>
    /// The name of the group of the shapes that are placed in no tab or card.
    /// </summary>
    public const string Default = "Content";

    /// <summary>
    /// Gets the tab and card names of a placement location, such as <c>Parts:1#Settings;2%Details</c>.
    /// </summary>
    /// <param name="location">The placement location.</param>
    /// <returns>The tab name, then the card name, of those the location has.</returns>
    public static IEnumerable<string> FromLocation(string location)
    {
        var placement = PlacementInfo.FromLocation(location);
        if (placement is null)
        {
            yield break;
        }

        if (placement.Tab.HasValue)
        {
            yield return placement.Tab.Name;
        }

        if (placement.Card.HasValue)
        {
            yield return placement.Card.Name;
        }
    }
}
