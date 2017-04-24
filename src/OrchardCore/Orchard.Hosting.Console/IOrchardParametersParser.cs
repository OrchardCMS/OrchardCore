using Orchard.Environment.Commands;

namespace Orchard.Hosting
{
    public interface IOrchardParametersParser
    {
        OrchardParameters Parse(CommandParameters parameters);
    }
}