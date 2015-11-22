using Orchard.Hosting.Parameters;
using System.Collections.Generic;

namespace Orchard.Hosting.HostContext
{
    public interface ICommandParametersParser
    {
        CommandParameters Parse(IEnumerable<string> args);
    }
}