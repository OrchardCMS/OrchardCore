using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OrchardCore.Admin.Models
{
    public interface IAdminSettings
    {
        public bool DisplayDarkMode { get; set; }
    }
}
