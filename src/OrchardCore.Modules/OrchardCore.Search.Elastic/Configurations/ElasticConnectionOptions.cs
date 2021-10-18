using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrchardCore.Search.Elastic.Configurations
{
    public class ElasticConnectionOptions
    {
        /// <summary>
        /// The server url.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Whether the configuration section exists.
        /// </summary>
        public bool ConfigurationExists { get; set; }
    }
}
