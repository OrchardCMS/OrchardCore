namespace OrchardCore.Media.AmazonS3;

internal static class AmazonS3Constants
{
    internal static class ValidationMessages
    {
        public const string BucketNameIsEmpty = "BucketName is required attribute for S3 storage.";
        public const string RegionAndServiceUrlAreEmpty = "Region or ServiceURL is a required attribute for S3 storage.";
    }

    internal static class AwsCredentialParamNames
    {
        public const string SecretKey = "SecretKey";
        public const string AccessKey = "AccessKey";
    }

    internal static class ConfigSections
    {
        public const string AmazonS3 = "OrchardCore_Media_AmazonS3";
        public const string AmazonS3ImageSharpCache = "OrchardCore_Media_AmazonS3_ImageSharp_Cache";
    }
}
