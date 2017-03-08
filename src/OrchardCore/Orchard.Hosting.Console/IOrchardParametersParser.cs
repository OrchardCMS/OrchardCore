using Orchard.Environment.Commands;
using Orchard.Environment.Commands.Parameters;

namespace Orchard.Hosting
{
    public interface IOrchardParametersParser
    {
        OrchardParameters Parse(CommandParameters parameters);
    }
}