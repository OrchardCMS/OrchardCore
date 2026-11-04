namespace OrchardCore.Contents.Deployment;

internal static class ContentDeploymentStepEditor
{
    internal static void Apply(ContentDeploymentStep step, string[] contentTypes, bool exportAsSetupRecipe)
    {
        step.ContentTypes = contentTypes?.ToArray() ?? [];
        step.ExportAsSetupRecipe = exportAsSetupRecipe;
    }
}
