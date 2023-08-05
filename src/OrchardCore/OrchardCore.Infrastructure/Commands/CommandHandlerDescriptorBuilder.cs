using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OrchardCore.Environment.Commands
{
    public class CommandHandlerDescriptorBuilder
    {
#pragma warning disable CA1822 // Mark members as static
        public CommandHandlerDescriptor Build(Type type)
#pragma warning restore CA1822 // Mark members as static
        {
            return new CommandHandlerDescriptor { Commands = CollectMethods(type) };
        }

        private static IEnumerable<CommandDescriptor> CollectMethods(Type type)
        {
            var methods = type
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName);

            foreach (var methodInfo in methods)
            {
                yield return BuildMethod(methodInfo);
            }
        }

        private static CommandDescriptor BuildMethod(MethodInfo methodInfo)
        {
            return new CommandDescriptor
            {
                Names = GetCommandNames(methodInfo),
                MethodInfo = methodInfo,
                HelpText = GetCommandHelpText(methodInfo)
            };
        }

        private static string GetCommandHelpText(MethodInfo methodInfo)
        {
            var attributes = methodInfo.GetCustomAttributes(typeof(CommandHelpAttribute), false/*inherit*/);
            if (attributes != null && attributes.Any())
            {
                return attributes.Cast<CommandHelpAttribute>().Single().HelpText;
            }
            return String.Empty;
        }

        private static string[] GetCommandNames(MethodInfo methodInfo)
        {
            var attributes = methodInfo.GetCustomAttributes(typeof(CommandNameAttribute), false/*inherit*/);
            if (attributes != null && attributes.Any())
            {
                return attributes.Cast<CommandNameAttribute>().Single().Commands;
            }

            return new[] { methodInfo.Name };
        }
    }
}
