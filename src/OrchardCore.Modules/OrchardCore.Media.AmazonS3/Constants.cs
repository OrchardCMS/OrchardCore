namespace OrchardCore.Media.AmazonS3;

internal static class Constants
{
    internal static class ValidationMessages
    {
        public const string BucketNameIsEmpty = "BucketName is required attribute for S3 Media";

        public const string RegionEndpointIsEmpty = "Region is required attribute for S3 Media";
    }

    internal static class AwsCredentialParamNames
    {
        public const string SecretKey = "SecretKey";
        public const string AccessKey = "AccessKey";
    }
}
