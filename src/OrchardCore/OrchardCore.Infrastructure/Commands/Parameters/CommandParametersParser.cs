using System;
using System.Collections.Generic;
using System.Security;

namespace OrchardCore.Environment.Commands.Parameters
{
    public class CommandParametersParser : ICommandParametersParser
    {
        [SecurityCritical]
        public CommandParameters Parse(IEnumerable<string> args)
        {
            var arguments = new List<string>();
            var switches = new Dictionary<string, string>();

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

                    switches.Add(switchName, switchValue);
                }
                else
                {
                    arguments.Add(arg);
                }
            }

            return new CommandParameters
            {
                Arguments = arguments,
                Switches = switches
            };
        }
    }
}
