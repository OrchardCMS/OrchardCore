using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Admin.Models
{
    public class AdminSettings
    {
        public bool DisplayMenuFilter { get; set; }
        public string BrandImageMedia { get; set; }
        public string FaviconMedia { get; set; }
        public string Head { get; set; }

        public string BrandImagePath
        {
            get
            {
                string path = null;

                if (!string.IsNullOrEmpty(BrandImageMedia))
                {
                    var media = JArray.Parse(BrandImageMedia);
                    path = media.FirstOrDefault()?["Path"]?.Value<string>();
                }

                return path;
            }
        }

        public string FaviconPath
        {
            get
            {
                string path = null;

                if (!string.IsNullOrEmpty(FaviconMedia))
                {
                    var media = JArray.Parse(FaviconMedia);
                    path = media.FirstOrDefault()?["Path"]?.Value<string>();
                }

                return path;
            }
        }
    }
}
