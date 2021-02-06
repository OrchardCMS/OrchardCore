using System;
using System.ComponentModel.DataAnnotations;
using OrchardCore.AdminMenu.Models;

namespace OrchardCore.AdminMenu.AdminNodes
{
    public class LinkAdminNode : AdminNode
    {
        [Required]
        public string LinkText { get; set; }

        [Required]
        public string LinkUrl { get; set; }

        public string IconClass { get; set; }

        /// <summary>
        /// The names of the permissions required to view this admin menu node
        /// </summary>
        public string[] PermissionNames { get; set; } = Array.Empty<string>();
    }
}
