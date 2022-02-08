namespace OrchardCore.BackgroundJobs.Services
{
    public interface IBackgroundJobIdGenerator
    {
        string GenerateUniqueId(IBackgroundJob backgroundJob);
    }
}
