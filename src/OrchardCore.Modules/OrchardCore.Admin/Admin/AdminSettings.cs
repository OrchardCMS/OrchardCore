using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Admin.Models
{
    public class AdminSettings
    {
        public bool DisplayMenuFilter { get; set; }
        public string BrandImageUrl { get; set; }
        public string FaviconUrl { get; set; }
        public string Head { get; set; }

        public string BrandImage
        {
            get
            {
                string path = null;

                if (!string.IsNullOrEmpty(BrandImageUrl))
                {
                    var media = JArray.Parse(BrandImageUrl);
                    path = media.FirstOrDefault()?["Path"]?.Value<string>();

                    if (!string.IsNullOrEmpty(path))
                    {
                        path = "/media/" + path;
                    }
                }

                return path;
            }
        }

        public string Favicon
        {
            get
            {
                string path = null;

                if (!string.IsNullOrEmpty(FaviconUrl))
                {
                    var media = JArray.Parse(FaviconUrl);
                    path = media.FirstOrDefault()?["Path"]?.Value<string>();

                    if (!string.IsNullOrEmpty(path))
                    {
                        path = "/media/" + path;
                    }
                }

                return path;
            }
        }
    }
}
