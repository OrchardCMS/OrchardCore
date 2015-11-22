using Orchard.Environment.Commands;
using Orchard.Hosting.HostContext;
using Orchard.Hosting.Parameters;
using System;
using System.IO;
using System.Linq;

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

        public CommandReturnCodes Run()
        {
            try
            {
                return DoRun();
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

        private CommandReturnCodes DoRun()
        {
            var context = CommandHostContext();
            if (context.DisplayUsageHelp)
            {
                DisplayUsageHelp();
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
            result = ExecuteInteractive(context);
            //}

            _commandHostContextProvider.Shutdown(context);
            return result;
        }

        private CommandHostContext CommandHostContext()
        {
            return _commandHostContextProvider.CreateContext();
        }

        private CommandReturnCodes ExecuteSingleCommand(CommandHostContext context)
        {
            return context.CommandHost.RunCommand(_input, _output, context.Arguments.Tenant, context.Arguments.Arguments.ToArray(), context.Arguments.Switches);
        }

        public CommandReturnCodes ExecuteInteractive(CommandHostContext context)
        {
            _output.WriteLine("Type \"?\" for help, \"exit\" to exit, \"cls\" to clear screen");
            while (true)
            {
                var command = ReadCommand(context);
                switch (command.ToLowerInvariant())
                {
                    case "quit":
                    case "q":
                    case "exit":
                    case "e":
                        return 0;
                    case "help":
                    case "?":
                        DisplayInteractiveHelp();
                        break;
                    case "cls":
                        //System.Console.Clear();
                        break;
                    default:
                        context = RunCommand(context, command);
                        break;
                }
            }
        }

        private string ReadCommand(CommandHostContext context)
        {
            _output.WriteLine();
            _output.Write("orchard> ");
            return _input.ReadLine();
        }

        private CommandHostContext RunCommand(CommandHostContext context, string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return context;

            CommandReturnCodes result = RunCommandInSession(context, command);
            if (result == context.RetryResult)
            {
                _commandHostContextProvider.Shutdown(context);
                context = CommandHostContext();
                result = RunCommandInSession(context, command);
                if (result != CommandReturnCodes.Ok)
                    _output.WriteLine("Command returned non-zero result: {0}", result);
            }
            return context;
        }

        private CommandReturnCodes RunCommandInSession(CommandHostContext context, string command)
        {
            var args = new OrchardParametersParser().Parse(new CommandParametersParser().Parse(new CommandLineParser().Parse(command)));
            return context.CommandHost.RunCommand(_input, _output, args.Tenant, args.Arguments.ToArray(), args.Switches);
        }

        private void DisplayInteractiveHelp()
        {
            _output.WriteLine("The Orchard command interpreter supports running a few built-in commands");
            _output.WriteLine("as well as specific commands from enabled features of an Orchard installation.");
            _output.WriteLine("");
            _output.WriteLine("The general syntax of commands is");
            _output.WriteLine("");
            _output.WriteLine("   <command-name> [arg1] ... [argn] [/switch1[:value1]] ... [/switchn[:valuen]]");
            _output.WriteLine("");
            _output.WriteLine("   <command-name>");
            _output.WriteLine("       Specifies the command to execute (the command name can be multiple words separated by spaces");
            _output.WriteLine("");
            _output.WriteLine("   [arg1] ... [argn]");
            _output.WriteLine("       Specifies additional arguments for the command");
            _output.WriteLine("");
            _output.WriteLine("   [/switch1[:value1]] ... [/switchn[:valuen]]");
            _output.WriteLine("       Specifies switches to apply to the command. Available switches generally ");
            _output.WriteLine("       depend on the command executed, with the exception of a few built-in ones.");
            _output.WriteLine("");
            _output.WriteLine("   Built-in commands");
            _output.WriteLine("   =================");
            _output.WriteLine("");
            _output.WriteLine("   help|h|?");
            _output.WriteLine("       Displays this message");
            _output.WriteLine("");
            _output.WriteLine("   exit|quit|e|q");
            _output.WriteLine("       Terminates the interactive session");
            _output.WriteLine("");
            _output.WriteLine("   cls");
            _output.WriteLine("       Clears the console screen");
            _output.WriteLine("");
            _output.WriteLine("   help commands");
            _output.WriteLine("       Displays the list of available commands");
            _output.WriteLine("");
            _output.WriteLine("   help <command-name>");
            _output.WriteLine("       Display help for a given command");
            _output.WriteLine("");
            _output.WriteLine("   Built-in switches");
            _output.WriteLine("   =================");
            _output.WriteLine("");
            _output.WriteLine("   /Verbose");
            _output.WriteLine("   /v");
            _output.WriteLine("       Turns on verbose output");
            _output.WriteLine("");
            _output.WriteLine("   /Tenant:tenant-name");
            _output.WriteLine("   /t:tenant-name");
            _output.WriteLine("       Specifies which tenant to run the command into. \"Default\" tenant by default.");
            _output.WriteLine("");
        }

        private void DisplayUsageHelp()
        {
            _output.WriteLine("Executes Orchard commands from a Orchard installation directory.");
            _output.WriteLine("");
            _output.WriteLine("Usage:");
            _output.WriteLine("   orchard.exe");
            _output.WriteLine("       Starts the Orchard command interpreter");
            _output.WriteLine("");
            _output.WriteLine("   orchard.exe <command-name> [arg1] ... [argn] [/switch1[:value1]] ... [/switchn[:valuen]]");
            _output.WriteLine("       Executes a single command");
            _output.WriteLine("");
            _output.WriteLine("   orchard.exe @response-file1 ... [@response-filen] [/switch1[:value1]] ... [/switchn[:valuen]]");
            _output.WriteLine("       Executes multiples commands from response files");
            _output.WriteLine("");
            _output.WriteLine(" where");
            _output.WriteLine("");
            _output.WriteLine("   <command-name>");
            _output.WriteLine("       Specifies the command to execute (the command name can be multiple words separated by spaces");
            _output.WriteLine("");
            _output.WriteLine("   [arg1] ... [argn]");
            _output.WriteLine("       Specify additional arguments for the command");
            _output.WriteLine("");
            _output.WriteLine("   [/switch1[:value1]] ... [/switchn[:valuen]]");
            _output.WriteLine("       Specify switches to apply to the command. Available switches generally ");
            _output.WriteLine("       depend on the command executed, with the exception of a few built-in ones.");
            _output.WriteLine("");
            _output.WriteLine("   [@response-file1] ... [@response-filen]");
            _output.WriteLine("       Specify one or more response files to be used for reading commands and switches.");
            _output.WriteLine("       A response file is a text file that contains one line per command to execute.");
            _output.WriteLine("");
            _output.WriteLine("   Built-in commands");
            _output.WriteLine("   =================");
            _output.WriteLine("");
            _output.WriteLine("   help commands");
            _output.WriteLine("       Display the list of available commands.");
            _output.WriteLine("");
            _output.WriteLine("   help <command-name>");
            _output.WriteLine("       Display help for a given command.");
            _output.WriteLine("");
            _output.WriteLine("   Built-in switches");
            _output.WriteLine("   =================");
            _output.WriteLine("");
            _output.WriteLine("   /WorkingDirectory:<physical-path>");
            _output.WriteLine("   /wd:<physical-path>");
            _output.WriteLine("       Specifies the orchard installation directory. The current directory is the default.");
            _output.WriteLine("");
            _output.WriteLine("   /Verbose");
            _output.WriteLine("   /v");
            _output.WriteLine("       Turn on verbose output");
            _output.WriteLine("");
            _output.WriteLine("   /VirtualPath:<virtual-path>");
            _output.WriteLine("   /vp:<virtual-path>");
            _output.WriteLine("       Virtual path to pass to the WebHost. Empty (i.e. root path) by default.");
            _output.WriteLine("");
            _output.WriteLine("   /Tenant:tenant-name");
            _output.WriteLine("   /t:tenant-name");
            _output.WriteLine("       Specifies which tenant to run the command into. \"Default\" tenant by default.");
            _output.WriteLine("");
            return;
        }
    }
}