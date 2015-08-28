using Orchard.Hosting.CommandConsole.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.Hosting.CommandConsole {
    public interface IOrchardParametersParser {
        OrchardParameters Parse(CommandParameters parameters);
    }
}
