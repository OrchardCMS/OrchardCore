// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.DotNet.Cli.CommandLine
{
    internal class CommandLineApplication
    {
        // Indicates whether the parser should throw an exception when it runs into an unexpected argument.
        // If this field is set to false, the parser will stop parsing when it sees an unexpected argument, and all
        // remaining arguments, including the first unexpected argument, will be stored in RemainingArguments property.
        private readonly bool _throwOnUnexpectedArg;

        public CommandLineApplication(bool throwOnUnexpectedArg = true)
        {
            _throwOnUnexpectedArg = throwOnUnexpectedArg;
            Options = new List<CommandOption>();
            Arguments = new List<CommandArgument>();
            Commands = new List<CommandLineApplication>();
            RemainingArguments = new List<string>();
            Invoke = () => 0;
        }

        public CommandLineApplication Parent { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public string Syntax { get; set; }
        public string Description { get; set; }
        public List<CommandOption> Options { get; private set; }
        public CommandOption OptionHelp { get; private set; }
        public CommandOption OptionVersion { get; private set; }
        public List<CommandArgument> Arguments { get; private set; }
        public List<string> RemainingArguments { get; private set; }
        public bool IsShowingInformation { get; protected set; }  // Is showing help or version?
        public Func<int> Invoke { get; set; }
        public Func<string> LongVersionGetter { get; set; }
        public Func<string> ShortVersionGetter { get; set; }
        public List<CommandLineApplication> Commands { get; private set; }
        public bool HandleResponseFiles { get; set; }
        public bool AllowArgumentSeparator { get; set; }

        public CommandLineApplication Command(string name, Action<CommandLineApplication> configuration,
            bool throwOnUnexpectedArg = true)
        {
            var command = new CommandLineApplication(throwOnUnexpectedArg) { Name = name, Parent = this };
            Commands.Add(command);
            configuration(command);
            return command;
        }

        public CommandOption Option(string template, string description, CommandOptionType optionType)
        {
            return Option(template, description, optionType, _ => { });
        }

        public CommandOption Option(string template, string description, CommandOptionType optionType, Action<CommandOption> configuration)
        {
            var option = new CommandOption(template, optionType) { Description = description };
            Options.Add(option);
            configuration(option);
            return option;
        }

        public CommandArgument Argument(string name, string description, bool multipleValues = false)
        {
            return Argument(name, description, _ => { }, multipleValues);
        }

        public CommandArgument Argument(string name, string description, Action<CommandArgument> configuration, bool multipleValues = false)
        {
            var lastArg = Arguments.LastOrDefault();
            if (lastArg != null && lastArg.MultipleValues)
            {
                var message = string.Format("The last argument '{0}' accepts multiple values. No more argument can be added.",
                    lastArg.Name);
                throw new InvalidOperationException(message);
            }

            var argument = new CommandArgument { Name = name, Description = description, MultipleValues = multipleValues };
            Arguments.Add(argument);
            configuration(argument);
            return argument;
        }

        public void OnExecute(Func<int> invoke)
        {
            Invoke = invoke;
        }

        public void OnExecute(Func<Task<int>> invoke)
        {
            Invoke = () => invoke().Result;
        }

        public int Execute(params string[] args)
        {
            CommandLineApplication command = this;
            CommandOption option = null;
            IEnumerator<CommandArgument> arguments = null;

            if (HandleResponseFiles)
            {
                args = ExpandResponseFiles(args).ToArray();
            }

            for (var index = 0; index < args.Length; index++)
            {
                var arg = args[index];
                var processed = false;
                if (!processed && option == null)
                {
                    string[] longOption = null;
                    string[] shortOption = null;

                    if (arg.StartsWith("--"))
                    {
                        longOption = arg.Substring(2).Split(new[] { ':', '=' }, 2);
                    }
                    else if (arg.StartsWith("-"))
                    {
                        shortOption = arg.Substring(1).Split(new[] { ':', '=' }, 2);
                    }
                    if (longOption != null)
                    {
                        processed = true;
                        string longOptionName = longOption[0];
                        option = command.Options.SingleOrDefault(opt => string.Equals(opt.LongName, longOptionName, StringComparison.Ordinal));

                        if (option == null)
                        {
                            if (string.IsNullOrEmpty(longOptionName) && !command._throwOnUnexpectedArg && AllowArgumentSeparator)
                            {
                                // a stand-alone "--" is the argument separator, so skip it and
                                // handle the rest of the args as unexpected args
                                index++;
                            }

                            HandleUnexpectedArg(command, args, index, argTypeName: "option");
                            break;
                        }

                        // If we find a help/version option, show information and stop parsing
                        if (command.OptionHelp == option)
                        {
                            command.ShowHelp();
                            return 0;
                        }
                        else if (command.OptionVersion == option)
                        {
                            command.ShowVersion();
                            return 0;
                        }

                        if (longOption.Length == 2)
                        {
                            if (!option.TryParse(longOption[1]))
                            {
                                command.ShowHint();
                                throw new CommandParsingException(command, $"Unexpected value '{longOption[1]}' for option '{option.LongName}'");
                            }
                            option = null;
                        }
                        else if (option.OptionType == CommandOptionType.NoValue || option.OptionType == CommandOptionType.BoolValue)
                        {
                            // No value is needed for this option
                            option.TryParse(null);
                            option = null;
                        }
                    }
                    if (shortOption != null)
                    {
                        processed = true;
                        option = command.Options.SingleOrDefault(opt => string.Equals(opt.ShortName, shortOption[0], StringComparison.Ordinal));

                        // If not a short option, try symbol option
                        if (option == null)
                        {
                            option = command.Options.SingleOrDefault(opt => string.Equals(opt.SymbolName, shortOption[0], StringComparison.Ordinal));
                        }

                        if (option == null)
                        {
                            HandleUnexpectedArg(command, args, index, argTypeName: "option");
                            break;
                        }

                        // If we find a help/version option, show information and stop parsing
                        if (command.OptionHelp == option)
                        {
                            command.ShowHelp();
                            return 0;
                        }
                        else if (command.OptionVersion == option)
                        {
                            command.ShowVersion();
                            return 0;
                        }

                        if (shortOption.Length == 2)
                        {
                            if (!option.TryParse(shortOption[1]))
                            {
                                command.ShowHint();
                                throw new CommandParsingException(command, $"Unexpected value '{shortOption[1]}' for option '{option.LongName}'");
                            }
                            option = null;
                        }
                        else if (option.OptionType == CommandOptionType.NoValue || option.OptionType == CommandOptionType.BoolValue)
                        {
                            // No value is needed for this option
                            option.TryParse(null);
                            option = null;
                        }
                    }
                }

                if (!processed && option != null)
                {
                    processed = true;
                    if (!option.TryParse(arg))
                    {
                        command.ShowHint();
                        throw new CommandParsingException(command, $"Unexpected value '{arg}' for option '{option.LongName}'");
                    }
                    option = null;
                }

                if (!processed && arguments == null)
                {
                    var currentCommand = command;
                    foreach (var subcommand in command.Commands)
                    {
                        if (string.Equals(subcommand.Name, arg, StringComparison.OrdinalIgnoreCase))
                        {
                            processed = true;
                            command = subcommand;
                            break;
                        }
                    }

                    // If we detect a subcommand
                    if (command != currentCommand)
                    {
                        processed = true;
                    }
                }
                if (!processed)
                {
                    if (arguments == null)
                    {
                        arguments = new CommandArgumentEnumerator(command.Arguments.GetEnumerator());
                    }
                    if (arguments.MoveNext())
                    {
                        processed = true;
                        arguments.Current.Values.Add(arg);
                    }
                }
                if (!processed)
                {
                    HandleUnexpectedArg(command, args, index, argTypeName: "command or argument");
                    break;
                }
            }

            if (option != null)
            {
                command.ShowHint();
                throw new CommandParsingException(command, $"Missing value for option '{option.LongName}'");
            }

            return command.Invoke();
        }

        // Helper method that adds a help option
        public CommandOption HelpOption(string template)
        {
            // Help option is special because we stop parsing once we see it
            // So we store it separately for further use
            OptionHelp = Option(template, "Show help information", CommandOptionType.NoValue);

            return OptionHelp;
        }

        public CommandOption VersionOption(string template,
                                           string shortFormVersion,
                                           string longFormVersion = null)
        {
            if (longFormVersion == null)
            {
                return VersionOption(template, () => shortFormVersion);
            }
            else
            {
                return VersionOption(template, () => shortFormVersion, () => longFormVersion);
            }
        }

        // Helper method that adds a version option
        public CommandOption VersionOption(string template,
                                           Func<string> shortFormVersionGetter,
                                           Func<string> longFormVersionGetter = null)
        {
            // Version option is special because we stop parsing once we see it
            // So we store it separately for further use
            OptionVersion = Option(template, "Show version information", CommandOptionType.NoValue);
            ShortVersionGetter = shortFormVersionGetter;
            LongVersionGetter = longFormVersionGetter ?? shortFormVersionGetter;

            return OptionVersion;
        }

        // Show short hint that reminds users to use help option
        public void ShowHint()
        {
            if (OptionHelp != null)
            {
                Console.WriteLine(string.Format("Specify --{0} for a list of available options and commands.", OptionHelp.LongName));
            }
        }

        // Show full help
        public void ShowHelp(string commandName = null)
        {
            var headerBuilder = new StringBuilder("Usage:");
            for (var cmd = this; cmd != null; cmd = cmd.Parent)
            {
                cmd.IsShowingInformation = true;
                headerBuilder.Insert(6, string.Format(" {0}", cmd.Name));
            }

            CommandLineApplication target;

            if (commandName == null || string.Equals(Name, commandName, StringComparison.OrdinalIgnoreCase))
            {
                target = this;
            }
            else
            {
                target = Commands.SingleOrDefault(cmd => string.Equals(cmd.Name, commandName, StringComparison.OrdinalIgnoreCase));

                if (target != null)
                {
                    headerBuilder.AppendFormat(" {0}", commandName);
                }
                else
                {
                    // The command name is invalid so don't try to show help for something that doesn't exist
                    target = this;
                }

            }

            var optionsBuilder = new StringBuilder();
            var commandsBuilder = new StringBuilder();
            var argumentsBuilder = new StringBuilder();

            if (target.Arguments.Any())
            {
                headerBuilder.Append(" [arguments]");

                argumentsBuilder.AppendLine();
                argumentsBuilder.AppendLine("Arguments:");
                var maxArgLen = MaxArgumentLength(target.Arguments);
                var outputFormat = string.Format("  {{0, -{0}}}{{1}}", maxArgLen + 2);
                foreach (var arg in target.Arguments)
                {
                    argumentsBuilder.AppendFormat(outputFormat, arg.Name, arg.Description);
                    argumentsBuilder.AppendLine();
                }
            }

            if (target.Options.Any())
            {
                headerBuilder.Append(" [options]");

                optionsBuilder.AppendLine();
                optionsBuilder.AppendLine("Options:");
                var maxOptLen = MaxOptionTemplateLength(target.Options);
                var outputFormat = string.Format("  {{0, -{0}}}{{1}}", maxOptLen + 2);
                foreach (var opt in target.Options)
                {
                    optionsBuilder.AppendFormat(outputFormat, opt.Template, opt.Description);
                    optionsBuilder.AppendLine();
                }
            }

            if (target.Commands.Any())
            {
                headerBuilder.Append(" [command]");

                commandsBuilder.AppendLine();
                commandsBuilder.AppendLine("Commands:");
                var maxCmdLen = MaxCommandLength(target.Commands);
                var outputFormat = string.Format("  {{0, -{0}}}{{1}}", maxCmdLen + 2);
                foreach (var cmd in target.Commands.OrderBy(c => c.Name))
                {
                    commandsBuilder.AppendFormat(outputFormat, cmd.Name, cmd.Description);
                    commandsBuilder.AppendLine();
                }

                if (OptionHelp != null)
                {
                    commandsBuilder.AppendLine();
                    commandsBuilder.AppendFormat("Use \"{0} [command] --help\" for more information about a command.", Name);
                    commandsBuilder.AppendLine();
                }
            }

            if (target.AllowArgumentSeparator)
            {
                headerBuilder.Append(" [[--] <arg>...]]");
            }

            headerBuilder.AppendLine();

            var nameAndVersion = new StringBuilder();
            nameAndVersion.AppendLine(GetFullNameAndVersion());
            nameAndVersion.AppendLine();

            Console.Write("{0}{1}{2}{3}{4}", nameAndVersion, headerBuilder, argumentsBuilder, optionsBuilder, commandsBuilder);
        }

        public void ShowVersion()
        {
            for (var cmd = this; cmd != null; cmd = cmd.Parent)
            {
                cmd.IsShowingInformation = true;
            }

            Console.WriteLine(FullName);
            Console.WriteLine(LongVersionGetter());
        }

        public string GetFullNameAndVersion()
        {
            return ShortVersionGetter == null ? FullName : string.Format("{0} {1}", FullName, ShortVersionGetter());
        }

        public void ShowRootCommandFullNameAndVersion()
        {
            var rootCmd = this;
            while (rootCmd.Parent != null)
            {
                rootCmd = rootCmd.Parent;
            }

            Console.WriteLine(rootCmd.GetFullNameAndVersion());
            Console.WriteLine();
        }

        private int MaxOptionTemplateLength(IEnumerable<CommandOption> options)
        {
            var maxLen = 0;
            foreach (var opt in options)
            {
                maxLen = opt.Template.Length > maxLen ? opt.Template.Length : maxLen;
            }
            return maxLen;
        }

        private int MaxCommandLength(IEnumerable<CommandLineApplication> commands)
        {
            var maxLen = 0;
            foreach (var cmd in commands)
            {
                maxLen = cmd.Name.Length > maxLen ? cmd.Name.Length : maxLen;
            }
            return maxLen;
        }

        private int MaxArgumentLength(IEnumerable<CommandArgument> arguments)
        {
            var maxLen = 0;
            foreach (var arg in arguments)
            {
                maxLen = arg.Name.Length > maxLen ? arg.Name.Length : maxLen;
            }
            return maxLen;
        }

        private void HandleUnexpectedArg(CommandLineApplication command, string[] args, int index, string argTypeName)
        {
            if (command._throwOnUnexpectedArg)
            {
                command.ShowHint();
                throw new CommandParsingException(command, $"Unrecognized {argTypeName} '{args[index]}'");
            }
            else
            {
                // All remaining arguments are stored for further use
                command.RemainingArguments.AddRange(new ArraySegment<string>(args, index, args.Length - index));
            }
        }

        private IEnumerable<string> ExpandResponseFiles(IEnumerable<string> args)
        {
            foreach (var arg in args)
            {
                if (!arg.StartsWith("@", StringComparison.Ordinal))
                {
                    yield return arg;
                }
                else
                {
                    var fileName = arg.Substring(1);

                    var responseFileArguments = ParseResponseFile(fileName);

                    // ParseResponseFile can suppress expanding this response file by
                    // returning null. In that case, we'll treat the response
                    // file token as a regular argument.

                    if (responseFileArguments == null)
                    {
                        yield return arg;
                    }
                    else
                    {
                        foreach (var responseFileArgument in responseFileArguments)
                            yield return responseFileArgument.Trim();
                    }
                }
            }
        }

        private IEnumerable<string> ParseResponseFile(string fileName)
        {
            if (!HandleResponseFiles)
                return null;

            if (!File.Exists(fileName))
            {
                throw new InvalidOperationException($"Response file '{fileName}' doesn't exist.");
            }

            return File.ReadLines(fileName);
        }

        private class CommandArgumentEnumerator : IEnumerator<CommandArgument>
        {
            private readonly IEnumerator<CommandArgument> _enumerator;

            public CommandArgumentEnumerator(IEnumerator<CommandArgument> enumerator)
            {
                _enumerator = enumerator;
            }

            public CommandArgument Current
            {
                get
                {
                    return _enumerator.Current;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }

            public void Dispose()
            {
                _enumerator.Dispose();
            }

            public bool MoveNext()
            {
                if (Current == null || !Current.MultipleValues)
                {
                    return _enumerator.MoveNext();
                }

                // If current argument allows multiple values, we don't move forward and
                // all later values will be added to current CommandArgument.Values
                return true;
            }

            public void Reset()
            {
                _enumerator.Reset();
            }
        }
    }
}