using System;

namespace OrchardCore.Contents.Models
{
    public class CommonPartSettings
    {
        public bool DisplayDateEditor { get; set; }

        public bool DisplayOwnerEditor { get; set; }

        public string[] Roles { get; set; } = Array.Empty<string>();
    }
}
