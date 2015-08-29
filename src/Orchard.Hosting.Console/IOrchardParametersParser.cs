using Orchard.Hosting.Console.Parameters;

namespace Orchard.Hosting.Console {
    public interface IOrchardParametersParser {
        OrchardParameters Parse(CommandParameters parameters);
    }
}
