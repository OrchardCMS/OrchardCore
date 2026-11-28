using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Localization.Data;

namespace OrchardCore.DisplayManagement.Descriptors;

/// <summary>
/// The names of the tabs and cards that placement groups shapes into, and their captions.
/// </summary>
/// <remarks>
/// A name is the identifier of its group, in placement rules and in the generated element ids, so only its caption is
/// translated: by the localizer, when the name is given as a localized string, such as <c>.Tab(S["Settings"])</c>, or
/// else with data localization, under the <see cref="DataLocalizationContext"/> context.
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

    /// <summary>
    /// Gets the caption of a tab: the display name that a shape of the tab is placed with, or else the data localization
    /// translation of its name.
    /// </summary>
    /// <param name="tab">The shapes of the tab, keyed by its name.</param>
    /// <param name="dataLocalizer">The data localizer.</param>
    public static string GetTabCaption(IGrouping<string, object> tab, IDataLocalizer dataLocalizer)
        => GetCaption(tab, static metadata => metadata.TabGrouping, dataLocalizer);

    /// <summary>
    /// Gets the caption of a card: the display name that a shape of the card is placed with, or else the data
    /// localization translation of its name.
    /// </summary>
    /// <param name="card">The shapes of the card, keyed by its name.</param>
    /// <param name="dataLocalizer">The data localizer.</param>
    public static string GetCardCaption(IGrouping<string, object> card, IDataLocalizer dataLocalizer)
        => GetCaption(card, static metadata => metadata.CardGrouping, dataLocalizer);

    private static string GetCaption(IGrouping<string, object> grouping, Func<ShapeMetadata, GroupingMetadata> select, IDataLocalizer dataLocalizer)
    {
        ArgumentNullException.ThrowIfNull(grouping);
        ArgumentNullException.ThrowIfNull(dataLocalizer);

        foreach (var item in grouping)
        {
            if (item is IShape shape && select(shape.Metadata).DisplayName is { Length: > 0 } displayName)
            {
                return displayName;
            }
        }

        return dataLocalizer[grouping.Key, DataLocalizationContext].Value;
    }
}
