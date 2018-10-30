using OrchardCore.AdminTrees.Models;

namespace OrchardCore.Contents.AdminNodes
{
    public class ContentTypesAdminNode : AdminNode
    {
        public bool ShowAll { get; set; }

        public string[] ContentTypes { get; set; } = new string[] { };
    }
}
