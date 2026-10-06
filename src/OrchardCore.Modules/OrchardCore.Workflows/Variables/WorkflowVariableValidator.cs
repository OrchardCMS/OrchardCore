using System.Text.RegularExpressions;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Variables;

/// <summary>
/// A problem with a variable declaration.
/// </summary>
public sealed class WorkflowVariableError
{
    /// <summary>
    /// The position of the variable in the list.
    /// </summary>
    public int Index { get; init; }

    /// <summary>
    /// The name of the variable, as declared.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// What is wrong.
    /// </summary>
    public string Message { get; init; }
}

/// <summary>
/// Checks variable declarations: names are unique identifiers, types exist, and defaults convert to their type.
/// </summary>
public sealed partial class WorkflowVariableValidator
{
    private readonly IWorkflowVariableTypeProvider _typeProvider;

    internal readonly IStringLocalizer S;

    public WorkflowVariableValidator(IWorkflowVariableTypeProvider typeProvider, IStringLocalizer<WorkflowVariableValidator> localizer)
    {
        _typeProvider = typeProvider;
        S = localizer;
    }

    /// <summary>
    /// Returns the problems of <paramref name="variables"/>, in their order.
    /// </summary>
    public IReadOnlyList<WorkflowVariableError> Validate(IList<WorkflowVariableDefinition> variables)
    {
        ArgumentNullException.ThrowIfNull(variables);

        var errors = new List<WorkflowVariableError>();
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var index = 0; index < variables.Count; index++)
        {
            var variable = variables[index];
            var message = Check(variable, names);

            if (message is not null)
            {
                errors.Add(new WorkflowVariableError { Index = index, Name = variable?.Name, Message = message });
            }
        }

        return errors;
    }

    private string Check(WorkflowVariableDefinition variable, HashSet<string> names)
    {
        if (string.IsNullOrWhiteSpace(variable?.Name))
        {
            return S["Enter a name."];
        }

        if (!IdentifierRegex().IsMatch(variable.Name))
        {
            return S["'{0}' isn't a valid name: use a letter or an underscore, then letters, digits or underscores.", variable.Name];
        }

        if (!names.Add(variable.Name))
        {
            return S["Another variable is named '{0}'.", variable.Name];
        }

        var type = _typeProvider.Get(variable.TypeName);

        if (type is null)
        {
            return S["The type '{0}' isn't available.", variable.TypeName ?? string.Empty];
        }

        if (variable.DefaultValue is not null && !type.TryCoerce(variable.DefaultValue, out _))
        {
            return S["The default value of '{0}' isn't a valid {1}.", variable.Name, type.DisplayName];
        }

        return null;
    }

    [GeneratedRegex("^[A-Za-z_][A-Za-z0-9_]*$")]
    private static partial Regex IdentifierRegex();
}
