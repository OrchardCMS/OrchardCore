using System.Collections.Generic;

namespace Orchard.Environment.Commands.Parameters
{
    public interface ICommandParametersParser
    {
        CommandParameters Parse(IEnumerable<string> args);
    }
}