using Orchard.Hosting.Console.Parameters;
using System.Collections.Generic;

namespace Orchard.Hosting.Console.HostContext {
    public interface ICommandParametersParser {
        CommandParameters Parse(IEnumerable<string> args);
    }
}
