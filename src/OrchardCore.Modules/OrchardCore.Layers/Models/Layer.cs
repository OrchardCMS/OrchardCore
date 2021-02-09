using System;
using OrchardCore.Rules;

namespace OrchardCore.Layers.Models
{
    public class Layer
    {
        public string Name { get; set; }

        /// <summary>
        /// This property is obsolete and layer rules should be used instead.
        /// It can be removed in a future version.
        /// </summary>
        #pragma warning disable 0618
        [Obsolete("The rule property is obsolete and LayerRule should be used instead.")]
        #pragma warning restore 0618
        public string Rule { get; set; }
        public string Description { get; set; }

        public Rule LayerRule { get; set; }
    }
}
