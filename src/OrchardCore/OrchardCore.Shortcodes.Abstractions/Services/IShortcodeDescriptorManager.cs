namespace OrchardCore.Shortcodes.Services;

public interface IShortcodeDescriptorManager
{
    Task<IEnumerable<ShortcodeDescriptor>> GetShortcodeDescriptors();
}
