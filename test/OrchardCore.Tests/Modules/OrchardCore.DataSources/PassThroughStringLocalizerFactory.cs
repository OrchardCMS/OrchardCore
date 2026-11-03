namespace OrchardCore.Tests.Modules.OrchardCore.DataSources;

/// <summary>
/// Creates localizers that return the strings they are given.
/// </summary>
internal sealed class PassThroughStringLocalizerFactory : IStringLocalizerFactory
{
    public IStringLocalizer Create(Type resourceSource) => new PassThroughStringLocalizer<object>();

    public IStringLocalizer Create(string baseName, string location) => new PassThroughStringLocalizer<object>();
}
