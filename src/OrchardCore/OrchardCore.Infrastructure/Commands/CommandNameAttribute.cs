using System;
using System.Text;

namespace OrchardCore.Environment.Commands
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

        public CommandHelpAttribute(params string[] helpText)
        {
            if (helpText.Length == 0)
            {
                return;
            }

            if (helpText.Length == 1)
            {
                HelpText = helpText[0];
                return;
            }

            StringBuilder builder = new StringBuilder();

            foreach (var value in helpText)
            {
                builder.AppendLine(value);
            }

            HelpText = builder.ToString();
        }

        public string HelpText { get; }
    }
}
