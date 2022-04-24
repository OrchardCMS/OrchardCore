namespace OrchardCore.Security.Options
{
    public class FrameAncestorsContentSecurityPolicySource
    {
        public static readonly string Any = "*";

        public static readonly string HttpSchema = "http:";

        public static readonly string HttpsSchema = "https:";

        public static readonly string DataSchema = "data:";

        public static readonly string MediaStreamSchema = "mediastream:";

        public static readonly string BlobSchema = "blob:";

        public static readonly string FileSystemSchema = "filesystem:";

        public static readonly string None = "'none'";

        public static readonly string Self = "'self'";
    }
}
