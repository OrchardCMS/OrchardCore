namespace OrchardCore.DisplayManagement.Razor;

public interface IOrchardDisplayHelper : IOrchardHelper
{
    IDisplayHelper DisplayHelper { get; }
}
