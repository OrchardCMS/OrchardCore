using System;
using System.Collections.Generic;
using System.Linq;
using OrchardCore.Navigation;

namespace OrchardCore.AdminMenu.Models
{
    public class AdminNode : MenuItem
    {
        public string UniqueId { get; set; } = Guid.NewGuid().ToString("n");
        public bool Enabled { get; set; } = true;

        public AdminNode GetMenuItemById(string id)
        {
            var tempStack = new Stack<AdminNode>(new AdminNode[] { this });

            while (tempStack.Any())
            {
                // evaluate first node
                AdminNode item = tempStack.Pop();
                if (item.UniqueId.Equals(id, StringComparison.OrdinalIgnoreCase))
                {
                    return item;
                }

                // not that one; continue with the rest.
                foreach (var i in item.Items)
                {
                    tempStack.Push((AdminNode)i);
                }
            }

            //not found
            return null;
        }

        // return boolean so that caller can check for success
        public bool RemoveMenuItem(AdminNode nodeToRemove)
        {
            var tempStack = new Stack<AdminNode>(new AdminNode[] { this });

            while (tempStack.Any())
            {
                // evaluate first
                MenuItem item = tempStack.Pop();
                if (item.Items.Contains(nodeToRemove))
                {
                    var items = item.Items.ToList();
                    items.Remove(nodeToRemove);

                    item.Items = items;
                    return true; //success
                }

                // not that one. continue
                foreach (var i in item.Items)
                {
                    tempStack.Push((AdminNode)i);
                }
            }

            // failure
            return false;
        }

        public bool InsertMenuItem(AdminNode nodeToInsert, MenuItem destinationNode, int position)
        {
            var tempStack = new Stack<AdminNode>(new AdminNode[] { this });
            while (tempStack.Any())
            {
                // evaluate first
                MenuItem node = tempStack.Pop();
                if (node.Equals(destinationNode))
                {
                    var items = node.Items.ToList();
                    items.Insert(position, nodeToInsert);

                    node.Items = items;
                    return true; // success
                }

                // not that one. continue
                foreach (var n in node.Items)
                {
                    tempStack.Push((AdminNode)n);
                }
            }

            // failure
            return false;
        }

        public AdminNode ShallowClone(AdminNode nodeToClone)
        {
            var tempStack = new Stack<AdminNode>(new AdminNode[] { this });

            while (tempStack.Any())
            {
                MenuItem item = tempStack.Pop();

                var index = item.Items.IndexOf(nodeToClone);
                if (index != -1)
                {
                    var items = item.Items.ToList();
                    var clone = nodeToClone.ShallowClone();

                    items[index] = clone;
                    item.Items = items;

                    return clone;
                }

                foreach (var i in item.Items)
                {
                    tempStack.Push((AdminNode)i);
                }
            }

            return null;
        }

        public AdminNode ShallowClone()
        {
            return MemberwiseClone() as AdminNode;
        }
    }
}
