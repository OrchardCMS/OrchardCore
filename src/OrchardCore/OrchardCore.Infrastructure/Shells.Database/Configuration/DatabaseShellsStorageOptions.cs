namespace OrchardCore.Shells.Database.Configuration
{
    public class DatabaseShellsStorageOptions
    {
        public string DatabaseProvider { get; set; }
        public string ConnectionString { get; set; }
        public string TablePrefix { get; set; }
    }
}
