using OrchardCore.AdminTrees.Models;

namespace OrchardCore.Lists.AdminNodes
{
    public class ListsAdminNode : AdminNode
    {
        public string[] ContentTypes { get; set; }

        public bool AddContentTypeAsParent { get; set; } = true;
    }
}
