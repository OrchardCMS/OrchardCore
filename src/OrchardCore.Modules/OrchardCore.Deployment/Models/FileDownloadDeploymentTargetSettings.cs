namespace OrchardCore.Deployment.Models
{
    public class FileDownloadDeploymentTargetSettings
    {
        /// <summary>
        /// The name of the rsa secret that can be used to encrypt exported data.
        /// </summary>
        public string RsaSecret { get; set; }
    }
}
