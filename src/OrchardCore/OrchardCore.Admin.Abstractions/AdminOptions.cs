namespace OrchardCore.Admin
{
    public class AdminOptions
    {
        private string _adminUrlPrefix = "Admin";

        public string AdminUrlPrefix
        {
            get => _adminUrlPrefix;
            set => _adminUrlPrefix = value.Trim(' ', '/');
        }
    }
}
