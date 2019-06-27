using System;

namespace OrchardCore.FileStorage.AliYunOss
{
    public abstract class OssStorageOptions
    {
        public string Endpoint { get; set; }
        public string BucketName { get; set; }
        public string BasePath { get; set; }
        public string AccessKeyId { get; set; }
        public string AccessKeySecret { get; set; }
    }
}
