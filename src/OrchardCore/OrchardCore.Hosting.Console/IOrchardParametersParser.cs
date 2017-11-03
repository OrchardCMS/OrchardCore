using OrchardCore.Environment.Commands;

namespace OrchardCore.Hosting
{
    public interface IOrchardParametersParser
    {
        OrchardParameters Parse(CommandParameters parameters);
    }
}