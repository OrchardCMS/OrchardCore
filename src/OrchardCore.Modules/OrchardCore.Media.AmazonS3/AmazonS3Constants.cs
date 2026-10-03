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
        public const string AmazonS3 = "Media:AmazonS3";
        public const string AmazonS3ImageCache = "Media:AmazonS3:ImageCache";

        // The 'OrchardCore_Media_AmazonS3' section is deprecated and will be removed in a future major version, use 'Media:AmazonS3' instead.
        public const string LegacyAmazonS3 = "OrchardCore_Media_AmazonS3";

        // The 'OrchardCore_Media_AmazonS3_ImageSharp_Cache' section is deprecated and will be removed in a future major version, use 'Media:AmazonS3:ImageCache' instead.
        public const string LegacyAmazonS3ImageCache = "OrchardCore_Media_AmazonS3_ImageSharp_Cache";
    }
}
