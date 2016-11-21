using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using DasMulli.Win32.ServiceUtils;
using Microsoft.DotNet.InternalAbstractions;
using System.IO;

namespace Orchard.Cms.Web
{
    public class Program
    {
        public const string RunAsServiceFlag = "--run-as-service";
        private const string RegisterServiceFlag = "--register-service";
        private const string UnregisterServiceFlag = "--unregister-service";
        private const string HelpFlag = "--?";

        private const string ServiceName = "Orchard 2";
        private const string ServiceDisplayName = "Orchard 2";
        private const string ServiceDescription = "Orchard 2 CMS running on .NET Core";

        public static void Main(string[] args)
        {
            try
            {
                if (args.Contains(RunAsServiceFlag))
                {
                    RunAsService(args);
                }
                else if (args.Contains(RegisterServiceFlag))
                {
                    RegisterService();
                }
                else if (args.Contains(UnregisterServiceFlag))
                {
                    UnregisterService();
                }
                else if (args.Contains(HelpFlag))
                {
                    DisplayHelp();
                }
                else
                {
                    RunInteractive(args);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error ocurred: {ex.Message}");
            }
        }

        private static void RunAsService(string[] args)
        {
            var appPath = ApplicationEnvironment.ApplicationBasePath;
            while (!File.Exists(appPath + "\\web.config"))
            {
                appPath = Directory.GetParent(appPath).FullName;
            }

            System.IO.Directory.SetCurrentDirectory(appPath);
            var orchardService = new OrchardCmsWin32Service(commandLineArguments: args.Where(a => a != RunAsServiceFlag).ToArray(), useInteractiveCommandLine: false);
            var serviceHost = new Win32ServiceHost(orchardService);
            serviceHost.Run();
        }

        private static void RunInteractive(string[] args)
        {
            var orchardService = new OrchardCmsWin32Service(commandLineArguments: args, useInteractiveCommandLine: true);
            orchardService.Start(new string[0], () => { });
            orchardService.Stop();
        }

        private static void RegisterService()
        {
            // Environment.GetCommandLineArgs() includes the current DLL from a "dotnet my.dll --register-service" call, which is not passed to Main()
            var remainingArgs = System.Environment.GetCommandLineArgs()
                .Where(arg => arg != RegisterServiceFlag)
                .Select(EscapeCommandLineArgument)
                .Append(RunAsServiceFlag);

            var host = Process.GetCurrentProcess().MainModule.FileName;
            if (!host.EndsWith("dotnet.exe", StringComparison.OrdinalIgnoreCase))
            {
                // For self-contained apps, skip the dll path
                remainingArgs = remainingArgs.Skip(1);
            }

            var fullServiceCommand = host + " " + string.Join(" ", remainingArgs);
            new Win32ServiceManager()
              .CreateService(ServiceName, ServiceDisplayName, ServiceDescription, fullServiceCommand, new Win32ServiceCredentials("NT SERVICE\\" + ServiceName, null), autoStart: true, startImmediately: true, errorSeverity: ErrorSeverity.Normal);

          Console.WriteLine($@"Successfully registered and started service ""{ServiceDisplayName}"" (""{ServiceDescription}"")");
        }

        private static void UnregisterService()
        {
            new Win32ServiceManager()
                                    .DeleteService(ServiceName);
            Console.WriteLine($@"Successfully unregistered service ""{ServiceDisplayName}"" (""{ServiceDescription}"")");
        }

        private static void DisplayHelp()
        {
            Console.WriteLine(ServiceDescription);
            Console.WriteLine();
            Console.WriteLine("Use one of the following options for running Orchard as a windows service or standalone:");
            Console.WriteLine("  --register-service        Registers and starts this program as a windows service named \"" + ServiceDisplayName + "\"");
            Console.WriteLine("                            All additional arguments will be passed to ASP.NET Core's WebHostBuilder.");
            Console.WriteLine("  --unregister-service      Removes the windows service created by --register-service.");
        }

        private static string EscapeCommandLineArgument(string arg)
        {
            // http://stackoverflow.com/a/6040946/784387
            arg = Regex.Replace(arg, @"(\\*)" + "\"", @"$1$1\" + "\"");
            arg = "\"" + Regex.Replace(arg, @"(\\+)$", @"$1$1") + "\"";
            return arg;
        }
    }
}