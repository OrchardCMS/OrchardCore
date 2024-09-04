using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Helpers;

public static class WorkflowScriptSyntaxExtensions
{
    public static string GetSyntaxName(this WorkflowScriptSyntax workflowScriptSyntax)
    {
        return workflowScriptSyntax switch
        {
            WorkflowScriptSyntax.JavaScript => "javascript",
            WorkflowScriptSyntax.Liquid => "liquid",
            _ => throw new NotSupportedException(),
        };
    }
}
