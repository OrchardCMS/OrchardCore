using System.Reflection;

namespace OrchardCore.Environment.Commands
{
    public class CommandDescriptor
    {
        public string[] Names { get; set; }
        public MethodInfo MethodInfo { get; set; }
        public string HelpText { get; set; }
    }
}
