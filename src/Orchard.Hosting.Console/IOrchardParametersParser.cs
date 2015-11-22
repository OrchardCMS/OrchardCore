using Orchard.Hosting.Parameters;

namespace Orchard.Hosting
{
    public interface IOrchardParametersParser
    {
        OrchardParameters Parse(CommandParameters parameters);
    }
}