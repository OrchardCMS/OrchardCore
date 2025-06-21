using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Environment.Commands.Builtin;

public class HelpCommand : DefaultCommandHandler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly CommandHandlerDescriptorBuilder _builder = new();

    public HelpCommand(IServiceProvider serviceProvider,
        IStringLocalizer<HelpCommand> localizer) : base(localizer)
    {
        _serviceProvider = serviceProvider;
    }

    [CommandName("help commands")]
    [CommandHelp("help commands", "\tDisplay help text for all available commands")]
    public async Task AllCommandsAsync()
    {
        await Context.Output.WriteLineAsync(S["List of available commands:"]).ConfigureAwait(false);
        await Context.Output.WriteLineAsync("---------------------------").ConfigureAwait(false);
        await Context.Output.WriteLineAsync().ConfigureAwait(false);

        var descriptors = GetCommandDescriptors().OrderBy(d => d.Names.First());

        foreach (var descriptor in descriptors)
        {
            await Context.Output.WriteLineAsync(GetHelpText(descriptor)).ConfigureAwait(false);
            await Context.Output.WriteLineAsync().ConfigureAwait(false);
        }
    }

    [CommandName("help")]
    [CommandHelp("help <command>", "\tDisplay help text for <command>")]
    public async Task SingleCommandAsync(string[] commandNameStrings)
    {
        var command = string.Join(" ", commandNameStrings);
        var descriptors = GetCommandDescriptors()
            .Where(t => t.Names.Any(x => x.StartsWith(command, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(d => d.Names);

        if (!descriptors.Any())
        {
            await Context.Output.WriteLineAsync(S["Command {0} doesn't exist", command]).ConfigureAwait(false);
        }
        else
        {
            foreach (var descriptor in descriptors)
            {
                await Context.Output.WriteLineAsync(GetHelpText(descriptor)).ConfigureAwait(false);
                await Context.Output.WriteLineAsync().ConfigureAwait(false);
            }
        }
    }
    private IEnumerable<CommandDescriptor> GetCommandDescriptors()
    {
        var commandhandlers = _serviceProvider.GetServices<ICommandHandler>();
        return commandhandlers.SelectMany(x => _builder.Build(x.GetType()).Commands);
    }

    private LocalizedString GetHelpText(CommandDescriptor descriptor)
    {
        if (string.IsNullOrEmpty(descriptor.HelpText))
        {
            return S["{0}.{1}: no help text", descriptor.MethodInfo.DeclaringType?.FullName, descriptor.MethodInfo.Name];
        }

        return S[descriptor.HelpText];
    }
}
