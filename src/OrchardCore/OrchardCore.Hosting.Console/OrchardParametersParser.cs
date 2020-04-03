using OrchardCore.Environment.Commands;
using OrchardCore.Environment.Commands.Parameters;
using System;
using System.Collections.Generic;
using System.Security;

namespace OrchardCore.Hosting
{
    public class OrchardParametersParser : IOrchardParametersParser
    {
        [SecurityCritical]
        public OrchardParameters Parse(CommandParameters parameters)
        {
            var result = new OrchardParameters
            {
                Arguments = new List<string>(),
                ResponseFiles = new List<string>(),
                Switches = new Dictionary<string, string>()
            };

            foreach (var arg in parameters.Arguments)
            {
                // @response-file
                if (arg[0] == '@')
                {
                    var filename = arg.Substring(1);
                    if (string.IsNullOrEmpty(filename))
                    {
                        throw new ArgumentException("Incorrect syntax: response file name can not be empty");
                    }
                    result.ResponseFiles.Add(filename);
                }
                // regular argument
                else
                {
                    result.Arguments.Add(arg);
                }
            }

            foreach (var sw in parameters.Switches)
            {
                // Built-in switches
                switch (sw.Key.ToLowerInvariant())
                {
                    case "v":
                    case "verbose":
                        bool verbose;
                        if (!bool.TryParse(sw.Value, out verbose))
                            verbose = true;
                        result.Verbose = verbose;
                        break;

                    case "t":
                    case "tenant":
                        result.Tenant = sw.Value;
                        break;

                    default:
                        result.Switches.Add(sw.Key, sw.Value);
                        break;
                }
            }

            return result;
        }
    }
}