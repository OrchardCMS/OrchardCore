using System;

namespace Orchard.Environment.Commands
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandNameAttribute : Attribute
    {
        public CommandNameAttribute(params string[] commandsAlias)
        {
            Commands = commandsAlias;
        }

        public string[] Commands { get; }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class CommandHelpAttribute : Attribute
    {
        public CommandHelpAttribute(string helpText)
        {
            HelpText = helpText;
        }

        public string HelpText { get; }
    }
}