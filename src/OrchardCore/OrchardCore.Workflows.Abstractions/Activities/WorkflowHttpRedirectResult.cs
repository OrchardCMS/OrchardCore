namespace OrchardCore.Workflows.Http;

public class WorkflowHttpRedirectResult
{
    public static readonly WorkflowHttpRedirectResult Instance = new();

    public string Url { get; set; }
}
