using Microsoft.Extensions.FileProviders;

namespace OrchardCore.DisplayManagement.Liquid.Internal
{
    /// <summary>
    /// Accessor to the <see cref="IFileProvider"/> used by <see cref="LiquidViewTemplate"/>.
    /// </summary>
    public interface ILiquidViewFileProviderAccessor
    {
        /// <summary>
        /// Gets the <see cref="IFileProvider"/> used to look up Liquid files.
        /// </summary>
        IFileProvider FileProvider { get; }
    }
}