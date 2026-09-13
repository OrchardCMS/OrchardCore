using OrchardCore.ContentManagement;

namespace OrchardCore.Contents.Deployment.AddToDeploymentPlan;

internal static class ContentItemDeploymentStepEditor
{
    internal static async Task<bool> TryApplyAsync(IContentManager contentManager, ContentItemDeploymentStep step, string contentItemId)
    {
        if (string.IsNullOrWhiteSpace(contentItemId) || await contentManager.GetAsync(contentItemId) is null)
        {
            return false;
        }
        step.ContentItemId = contentItemId;
        return true;
    }
}
