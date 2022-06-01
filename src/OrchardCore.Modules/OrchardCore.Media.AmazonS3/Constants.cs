namespace OrchardCore.Media.AmazonS3;

internal static class Constants
{
    internal static class ValidationMessages
    {
        public const string BucketNameIsEmpty = "BucketName is required attribute for S3 Media";

        public const string SecretKeyIsEmpty =
            "SecretKey is required attribute for S3 Media, make sure it exists in Credentials section or ProfileName you specified";

        public const string AccessKeyIdIsEmpty =
            "AccessKeyId is required attribute for S3 Media, make sure it exists in Credentials section or ProfileName you specified";

        public const string RegionEndpointIsEmpty =
            "Region is required attribute for S3 Media, make sure it exists in Credentials section or ProfileName you specified";
    }
}
