using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrchardCore.SecureSocketsLayer.ViewModels
{
    public class SslSettingsViewModel
    {
        public bool IsHttpsRequest { get; set; }
        public bool RequireHttps { get; set; }
        public bool RequireHttpsPermanent { get; set; }
        public int? SslPort { get; set; }
    }
}
