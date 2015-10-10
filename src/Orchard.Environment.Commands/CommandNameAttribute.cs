using System;

namespace Orchard.Environment.Commands {
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandNameAttribute : Attribute {
        private readonly string _commandAlias;

        public CommandNameAttribute(string commandAlias) {
            _commandAlias = commandAlias;
        }

        public string Command {
            get { return _commandAlias; }
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class CommandHelpAttribute : Attribute {
        private readonly string _helpText;

        public CommandHelpAttribute(string helpText) {
            _helpText = helpText;
        }

        public string HelpText { get { return _helpText; } }
    }
}
