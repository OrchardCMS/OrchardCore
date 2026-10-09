using System.Reflection;

namespace OrchardCore.Environment.Extensions;

public class ExtensionEntry
{
    public IExtensionInfo ExtensionInfo { get; set; } = null!;
    public Assembly? Assembly { get; set; }
    public IEnumerable<Type> ExportedTypes { get; set; } = null!;
    public bool IsError { get; set; }
}
