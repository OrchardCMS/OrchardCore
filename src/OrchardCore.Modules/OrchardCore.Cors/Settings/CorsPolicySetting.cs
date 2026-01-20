namespace OrchardCore.Cors.Settings
{
    public class CorsPolicySetting
    {
        public string Name { get; set; }

        public bool AllowAnyOrigin { get; set; }

        public string[] AllowedOrigins { get; set; }

        public bool AllowAnyHeader { get; set; }

        public string[] AllowedHeaders { get; set; }

        public bool AllowAnyMethod { get; set; }

        public string[] AllowedMethods { get; set; }

        public bool AllowCredentials { get; set; }

        public bool IsDefaultPolicy { get; set; }
    }
}
