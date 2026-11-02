namespace OrchardCore.Tests.Modules.OrchardCore.Workflows.Variables;

/// <summary>
/// Creates localizers that return the text they are given, formatted with its arguments.
/// </summary>
internal sealed class PassThroughStringLocalizerFactory : IStringLocalizerFactory
{
    public IStringLocalizer Create(Type resourceSource) => new PassThroughStringLocalizer<object>();

    public IStringLocalizer Create(string baseName, string location) => new PassThroughStringLocalizer<object>();
}
