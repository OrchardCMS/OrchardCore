using OrchardCore.Shortcodes.Models;

namespace OrchardCore.Shortcodes.ViewModels;

public class DataSourceEntry
{
    public DataSourceEntry(ShortcodeTemplate shortcodeTemplate = null)
    {
        ArgumentNullException.ThrowIfNull(shortcodeTemplate);
        //DataSource = dataSource;
        ShortcodeTemplate = shortcodeTemplate;
    }

/*     /// <summary>
    /// The integration data from code
    /// </summary>
    public IDataSource DataSource { get; } */

    /// <summary>
    /// Provoded by the industry site.
    /// Maybe null if not yet created on Industry site.
    /// </summary>
    public ShortcodeTemplate ShortcodeTemplate { get; }
}
