namespace OrchardCore.Shortcodes.Services;

public interface IShortcodeDescriptorProvider
{
    Task<IEnumerable<ShortcodeDescriptor>> DiscoverAsync();
}
