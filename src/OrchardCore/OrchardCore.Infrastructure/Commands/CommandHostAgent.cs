using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Modules;

namespace OrchardCore.Environment.Commands
{
    public class CommandHostAgent
    {
        private readonly IShellHost _orchardHost;
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly IStringLocalizer S;

        public CommandHostAgent(IShellHost orchardHost,
            IShellSettingsManager shellSettingsManager,
            IStringLocalizer localizer)
        {
            _orchardHost = orchardHost;
            _shellSettingsManager = shellSettingsManager;

            S = localizer;
        }

        public Task<CommandReturnCodes> RunSingleCommandAsync(TextReader input, TextWriter output, string tenant, string[] args, IDictionary<string, string> switches)
        {
            return RunCommandAsync(input, output, tenant, args, switches);
        }

        public async Task<CommandReturnCodes> RunCommandAsync(TextReader input, TextWriter output, string tenant, string[] args, IDictionary<string, string> switches)
        {
            try
            {
                tenant = tenant ?? ShellHelper.DefaultShellName;

                using (var env = await CreateStandaloneEnvironmentAsync(tenant))
                {
                    var commandManager = env.ServiceProvider.GetService<ICommandManager>();

                    var parameters = new CommandParameters
                    {
                        Arguments = args,
                        Switches = switches,
                        Input = input,
                        Output = output
                    };

                    await commandManager.ExecuteAsync(parameters);
                }

                return CommandReturnCodes.Ok;
            }
            catch (Exception ex)
            {
                if (ex.IsFatal())
                {
                    throw;
                }
                if (ex is TargetInvocationException &&
                    ex.InnerException != null)
                {
                    // If this is an exception coming from reflection and there is an innerexception which is the actual one, redirect
                    ex = ex.InnerException;
                }
                await OutputExceptionAsync(output, S["Error executing command \"{0}\"", string.Join(" ", args)], ex);
                return CommandReturnCodes.Fail;
            }
        }

        private async Task OutputExceptionAsync(TextWriter output, LocalizedString title, Exception exception)
        {
            // Display header
            await output.WriteLineAsync();
            await output.WriteLineAsync(title);

            // Push exceptions in a stack so we display from inner most to outer most
            var errors = new Stack<Exception>();
            for (var scan = exception; scan != null; scan = scan.InnerException)
            {
                errors.Push(scan);
            }

            // Display inner most exception details
            exception = errors.Peek();
            await output.WriteLineAsync(S["--------------------------------------------------------------------------------"]);
            await output.WriteLineAsync();
            await output.WriteLineAsync(S["{0}", exception.Message]);
            await output.WriteLineAsync();

            if (!(exception.InnerException == null))
            {
                await output.WriteLineAsync(S["Exception Details: {0}: {1}", exception.GetType().FullName, exception.Message]);
                await output.WriteLineAsync();
                await output.WriteLineAsync(S["Stack Trace:"]);
                await output.WriteLineAsync();

                // Display exceptions from inner most to outer most
                foreach (var error in errors)
                {
                    await output.WriteLineAsync(S["[{0}: {1}]", error.GetType().Name, error.Message]);
                    await output.WriteLineAsync(S["{0}", error.StackTrace]);
                    await output.WriteLineAsync();
                }
            }

            // Display footer
            await output.WriteLineAsync("--------------------------------------------------------------------------------");
            await output.WriteLineAsync();
        }

        private Task<ShellContext> CreateStandaloneEnvironmentAsync(string tenant)
        {
            // Retrieve settings for speficified tenant.
            var settingsList = _shellSettingsManager.LoadSettings();
            if (settingsList.Any())
            {
                var settings = settingsList.SingleOrDefault(s => string.Equals(s.Name, tenant, StringComparison.OrdinalIgnoreCase));
                if (settings == null)
                {
                    throw new Exception(S["Tenant {0} does not exist", tenant]);
                }

                return _orchardHost.CreateShellContextAsync(settings);
            }
            else
            {
                // In case of an uninitialized site (no default settings yet), we create a default settings instance.
                var settings = new ShellSettings { Name = ShellHelper.DefaultShellName, State = TenantState.Uninitialized };
                return _orchardHost.CreateShellContextAsync(settings);
            }
        }
    }
}
