namespace OrchardCore.Lists.AdminNodes
{
    public class ListsAdminNodeViewModel
    {
        public string[] ContentTypes { get; set; }

        public bool Enabled { get; set; }

        public string CustomClasses { get; set; }

        public bool AddContentTypeAsParent { get; set; }
    }
}
