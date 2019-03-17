using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.Sitemaps.Services
{
    public interface ISitemapIdGenerator
    {
        string GenerateUniqueId();
    }
}
