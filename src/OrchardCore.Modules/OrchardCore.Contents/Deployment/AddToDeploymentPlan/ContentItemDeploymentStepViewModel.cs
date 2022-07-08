using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Contents.Deployment.AddToDeploymentPlan
{
    public class ContentItemDeploymentStepViewModel
    {
        [Required]
        public string ContentItemId { get; set; }
    }
}
