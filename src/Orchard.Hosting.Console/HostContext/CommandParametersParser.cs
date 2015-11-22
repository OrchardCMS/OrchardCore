using Orchard.Hosting.Parameters;
using System;
using System.Collections.Generic;
using System.Security;

namespace Orchard.Hosting.HostContext
{
    public class CommandParametersParser : ICommandParametersParser
    {
        [SecurityCritical]
        public CommandParameters Parse(IEnumerable<string> args)
        {
            var result = new CommandParameters
            {
                Arguments = new List<string>(),
                Switches = new Dictionary<string, string>()
            };

            foreach (var arg in args)
            {
                // Switch?
                if (arg[0] == '/')
                {
                    int index = arg.IndexOf(':');
                    var switchName = (index < 0 ? arg.Substring(1) : arg.Substring(1, index - 1));
                    var switchValue = (index < 0 || index >= arg.Length ? string.Empty : arg.Substring(index + 1));

                    if (string.IsNullOrEmpty(switchName))
                    {
                        throw new ArgumentException(string.Format("Invalid switch syntax: \"{0}\". Valid syntax is /<switchName>[:<switchValue>].", arg));
                    }

                    result.Switches.Add(switchName, switchValue);
                }
                else
                {
                    result.Arguments.Add(arg);
                }
            }

            return result;
        }
    }
}