namespace OrchardCore.Workflows.Services
{
    // TODO: Reuse the one from ContentManagement once it's moved to a more central location. See: https://github.com/OrchardCMS/OrchardCore/issues/1473
    public interface IIdGenerator
    {
        string GenerateUniqueId();
    }
}
