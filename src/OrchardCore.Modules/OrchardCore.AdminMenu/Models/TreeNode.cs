using System;
using System.Collections.Generic;
using System.Text;
using OrchardCore.Environment.Navigation;

namespace OrchardCore.ContentTree.Models
{
    public abstract class TreeNode : MenuItem
    {
        public TreeNode()
        {
            
        }
        public string TreeNodeId { get; set; }
        public string Name { get; set; }

        public List<TreeNode> TreeNodes { get; set; }
    }
}
