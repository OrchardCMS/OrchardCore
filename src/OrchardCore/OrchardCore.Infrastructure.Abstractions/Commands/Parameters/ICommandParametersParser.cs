using System.Collections.Generic;

namespace OrchardCore.Environment.Commands.Parameters
{
    public interface ICommandParametersParser
    {
        CommandParameters Parse(IEnumerable<string> args);
    }
}
