using Orchard.Environment.Commands;
using Orchard.Hosting.HostContext;
using Orchard.Environment.Commands.Parameters;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.Hosting
{
    public class OrchardHost
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly OrchardConsoleLogger _logger;
        private readonly ICommandHostContextProvider _commandHostContextProvider;

        private readonly TextReader _input;
        private readonly TextWriter _output;

        public OrchardHost(
            IServiceProvider serviceProvider,
            TextReader input,
            TextWriter output,
            string[] args)
        {
            _serviceProvider = serviceProvider;
            _input = input;
            _output = output;
            _logger = new OrchardConsoleLogger(input, output);

            _commandHostContextProvider = new CommandHostContextProvider(
                serviceProvider, _logger, args);
        }

        public async Task<CommandReturnCodes> RunAsync()
        {
            try
            {
                return await DoRunAsync();
            }
            catch (Exception e)
            {
                _logger.LogError("Error:");
                for (; e != null; e = e.InnerException)
                {
                    _logger.LogError("  {0}", e.Message);
                }
                return CommandReturnCodes.Fail;
            }
        }

        private async Task<CommandReturnCodes> DoRunAsync()
        {
            var context = CommandHostContext();
            if (context.DisplayUsageHelp)
            {
                await DisplayUsageHelpAsync();
                return CommandReturnCodes.Ok;
            }
            if (context.StartSessionResult == CommandReturnCodes.Fail)
            {
                _commandHostContextProvider.Shutdown(context);
                return context.StartSessionResult;
            }

            CommandReturnCodes result = CommandReturnCodes.Fail;
            //if (context.Arguments.Arguments.Any())
            //    result = ExecuteSingleCommand(context);
            //else if (context.Arguments.ResponseFiles.Any())
            //    result = ExecuteResponseFiles(context);
            //else {
            result = await ExecuteInteractiveAsync(context);
            //}

            _commandHostContextProvider.Shutdown(context);
            return result;
        }

        private CommandHostContext CommandHostContext()
        {
            return _commandHostContextProvider.CreateContext();
        }

        private async Task<CommandReturnCodes> ExecuteSingleCommandAsync(CommandHostContext context)
        {
            return await context.CommandHost.RunCommandAsync(_input, _output, context.Arguments.Tenant, context.Arguments.Arguments.ToArray(), context.Arguments.Switches);
        }
        
        public async Task<CommandReturnCodes> ExecuteInteractiveAsync(CommandHostContext context)
        {
            await _output.WriteLineAsync("Type \"?\" for help, \"exit\" to exit, \"cls\" to clear screen");
            while (true)
            {
                var command = await ReadCommandAsync(context);
                switch (command?.ToLowerInvariant())
                {
                    case "quit":
                    case "q":
                    case "exit":
                    case "e":
                        return 0;
                    case "help":
                    case "?":
                        await DisplayInteractiveHelpAsync();
                        break;
                    case "cls":
                        System.Console.Clear();
                        break;
                    default:
                        context = await RunCommandAsync(context, command);
                        break;
                }
            }
        }

        private async Task<string> ReadCommandAsync(CommandHostContext context)
        {
            await _output.WriteLineAsync();
            await _output.WriteAsync("orchard> ");
            return await _input.ReadLineAsync();
        }

        private async Task<CommandHostContext> RunCommandAsync(CommandHostContext context, string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return context;
            }

            CommandReturnCodes result = await RunCommandInSessionAsync(context, command);
            if (result == context.RetryResult)
            {
                _commandHostContextProvider.Shutdown(context);
                context = CommandHostContext();
                result = await RunCommandInSessionAsync(context, command);
                if (result != CommandReturnCodes.Ok)
                {
                    await _output.WriteLineAsync($"Command returned non-zero result: {result}");
                }
            }
            return context;
        }

        private async Task<CommandReturnCodes> RunCommandInSessionAsync(CommandHostContext context, string command)
        {
            var args = new OrchardParametersParser().Parse(new CommandParametersParser().Parse(new CommandParser().Parse(command)));
            return await context.CommandHost.RunCommandAsync(_input, _output, args.Tenant, args.Arguments.ToArray(), args.Switches);
        }

        private async Task DisplayInteractiveHelpAsync()
        {
            await _output.WriteLineAsync("The Orchard command interpreter supports running a few built-in commands");
            await _output.WriteLineAsync("as well as specific commands from enabled features of an Orchard installation.");
            await _output.WriteLineAsync();
            await _output.WriteLineAsync("The general syntax of commands is");
            await _output.WriteLineAsync();
            await _output.WriteLineAsync("   <command-name> [arg1] ... [argn] [/switch1[:value1]] ... [/switchn[:valuen]]");
            await _output.WriteLineAsync();
            await _output.WriteLineAsync("   <command-name>");
            await _output.WriteLineAsync("       Specifies the command to execute (the command name can be multiple words separated by spaces");
            await _output.WriteLineAsync();
            await _output.WriteLineAsync("   [arg1] ... [argn]");
            await _output.WriteLineAsync("       Specifies additional arguments for the command");
            await _output.WriteLineAsync();
            await _output.WriteLineAsync("   [/switch1[:value1]] ... [/switchn[:valuen]]");
            await _output.WriteLineAsync("       Specifies switches to apply to the command. Available switches generally ");
            await _output.WriteLineAsync("       depend on the command executed, with the exception of a few built-in ones.");
            await _output.WriteLineAsync();
            await _output.WriteLineAsync("   Built-in commands");
            await _output.WriteLineAsync("   =================");
            await _output.WriteLineAsync();
            await _output.WriteLineAsync("   help|h|?");
            await _output.WriteLineAsync("       Displays this message");
            await _output.WriteLineAsync();
            await _output.WriteLineAsync("   exit|quit|e|q");
            await _output.WriteLineAsync("       Terminates the interactive session");
            await _output.WriteLineAsync();
            await _output.WriteLineAsync("   cls");
            await _output.WriteLineAsync("       Clears the console screen");
            await _output.WriteLineAsync();
            await _output.WriteLineAsync("   help commands");
            await _output.WriteLineAsync("       Displays the list of available commands");
            await _output.WriteLineAsync();
            await _output.WriteLineAsync("   help <command-name>");
            await _output.WriteLineAsync("       Display help for a given command");
            await _output.WriteLineAsync();
            await _output.WriteLineAsync("   Built-in switches");
            await _output.WriteLineAsync("   =================");
            await _output.WriteLineAsync();
            await _output.WriteLineAsync("   /Verbose");
            await _output.WriteLineAsync("   /v");
            await _output.WriteLineAsync("       Turns on verbose output");
            await _output.WriteLineAsync();
            await _output.WriteLineAsync("   /Tenant:tenant-name");
            await _output.WriteLineAsync("   /t:tenant-name");
            await _output.WriteLineAsync("       Specifies which tenant to run the command into. \"Default\" tenant by default.");
            await _output.WriteLineAsync();
        }

        private async Task DisplayUsageHelpAsync()
        {
            await _output.WriteLineAsync("Executes Orchard commands from a Orchard installation directory.");
            await _output.WriteLineAsync();
            await _output.WriteLineAsync("Usage:");
            await _output.WriteLineAsync("   orchard.exe");
            await _output.WriteLineAsync("       Starts the Orchard command interpreter");
            await _output.WriteLineAsync();
            await _output.WriteLineAsync("   orchard.exe <command-name> [arg1] ... [argn] [/switch1[:value1]] ... [/switchn[:valuen]]");
            await _output.WriteLineAsync("       Executes a single command");
            await _output.WriteLineAsync();
            await _output.WriteLineAsync("   orchard.exe @response-file1 ... [@response-filen] [/switch1[:value1]] ... [/switchn[:valuen]]");
            await _output.WriteLineAsync("       Executes multiples commands from response files");
            await _output.WriteLineAsync();
            await _output.WriteLineAsync(" where");
            await _output.WriteLineAsync();
            await _output.WriteLineAsync("   <command-name>");
            await _output.WriteLineAsync("       Specifies the command to execute (the command name can be multiple words separated by spaces");
            await _output.WriteLineAsync();
            await _output.WriteLineAsync("   [arg1] ... [argn]");
            await _output.WriteLineAsync("       Specify additional arguments for the command");
            await _output.WriteLineAsync();
            await _output.WriteLineAsync("   [/switch1[:value1]] ... [/switchn[:valuen]]");
            await _output.WriteLineAsync("       Specify switches to apply to the command. Available switches generally ");
            await _output.WriteLineAsync("       depend on the command executed, with the exception of a few built-in ones.");
            await _output.WriteLineAsync();
            await _output.WriteLineAsync("   [@response-file1] ... [@response-filen]");
            await _output.WriteLineAsync("       Specify one or more response files to be used for reading commands and switches.");
            await _output.WriteLineAsync("       A response file is a text file that contains one line per command to execute.");
            await _output.WriteLineAsync();
            await _output.WriteLineAsync("   Built-in commands");
            await _output.WriteLineAsync("   =================");
            await _output.WriteLineAsync();
            await _output.WriteLineAsync("   help commands");
            await _output.WriteLineAsync("       Display the list of available commands.");
            await _output.WriteLineAsync();
            await _output.WriteLineAsync("   help <command-name>");
            await _output.WriteLineAsync("       Display help for a given command.");
            await _output.WriteLineAsync();
            await _output.WriteLineAsync("   Built-in switches");
            await _output.WriteLineAsync("   =================");
            await _output.WriteLineAsync();
            await _output.WriteLineAsync("   /WorkingDirectory:<physical-path>");
            await _output.WriteLineAsync("   /wd:<physical-path>");
            await _output.WriteLineAsync("       Specifies the orchard installation directory. The current directory is the default.");
            await _output.WriteLineAsync();
            await _output.WriteLineAsync("   /Verbose");
            await _output.WriteLineAsync("   /v");
            await _output.WriteLineAsync("       Turn on verbose output");
            await _output.WriteLineAsync();
            await _output.WriteLineAsync("   /VirtualPath:<virtual-path>");
            await _output.WriteLineAsync("   /vp:<virtual-path>");
            await _output.WriteLineAsync("       Virtual path to pass to the WebHost. Empty (i.e. root path) by default.");
            await _output.WriteLineAsync();
            await _output.WriteLineAsync("   /Tenant:tenant-name");
            await _output.WriteLineAsync("   /t:tenant-name");
            await _output.WriteLineAsync("       Specifies which tenant to run the command into. \"Default\" tenant by default.");
            await _output.WriteLineAsync();
        }
    }
}