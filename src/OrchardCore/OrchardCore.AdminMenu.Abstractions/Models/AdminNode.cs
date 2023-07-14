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
                // Evaluate first node.
                var item = tempStack.Pop();
                if (item.UniqueId.Equals(id, StringComparison.OrdinalIgnoreCase))
                {
                    return item;
                }

                // Not that one, continue with the rest.
                foreach (var i in item.Items)
                {
                    tempStack.Push((AdminNode)i);
                }
            }

            // Not found.
            return null;
        }

        // Return boolean so that caller can check for success.
        public bool RemoveMenuItem(AdminNode nodeToRemove)
        {
            var tempStack = new Stack<AdminNode>(new AdminNode[] { this });

            while (tempStack.Any())
            {
                // Evaluate first.
                var item = tempStack.Pop();
                if (item.Items.Contains(nodeToRemove))
                {
                    item.Items.Remove(nodeToRemove);
                    return true; // Success.
                }

                // Not that one, continue.
                foreach (var i in item.Items)
                {
                    tempStack.Push((AdminNode)i);
                }
            }

            // Failure.
            return false;
        }

        public bool InsertMenuItem(AdminNode nodeToInsert, MenuItem destinationNode, int position)
        {
            var tempStack = new Stack<AdminNode>(new AdminNode[] { this });
            while (tempStack.Any())
            {
                // Evaluate first.
                var node = tempStack.Pop();
                if (node.Equals(destinationNode))
                {
                    node.Items.Insert(position, nodeToInsert);
                    return true; // Success.
                }

                // Not that one, continue.
                foreach (var n in node.Items)
                {
                    tempStack.Push((AdminNode)n);
                }
            }

            // Failure.
            return false;
        }
    }
}
