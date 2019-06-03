using System.ComponentModel.DataAnnotations;

namespace OrchardCore.LetsEncrypt.Settings
{
    public class LetsEncryptAzureAuthSettings
    {
        [Required]
        public string Tenant { get; set; }
        [Required]
        public string SubscriptionId { get; set; }
        [Required]
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        [Required]
        public string ResourceGroupName { get; set; }
        public string ServicePlanResourceGroupName { get; set; }
        public bool UseIPBasedSSL { get; set; }
        [Required]
        public string WebAppName { get; set; }
        public string SiteSlotName { get; set; }
    }
}
