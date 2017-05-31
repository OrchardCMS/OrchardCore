using DotLiquid;
using System.Collections.Concurrent;

namespace Orchard.Liquid
{
    public class LiquidManager : ILiquidManager
    {
        private static Template Empty = Template.Parse("");

        // The caches is a singleton per tenant so that they can be cleared when the tenant
        // is restarted.
        private ConcurrentDictionary<string, Template> _templates = new ConcurrentDictionary<string, Template>();
        
        public Template GetTemplate(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return null;
            }

            if (_templates.TryGetValue(source, out var template))
            {
                return template;
            }
            else
            {
                try
                {
                    template = Template.Parse(source);
                    template.MakeThreadSafe();
                    template = _templates.GetOrAdd(source, template);
                    return template;
                }
                catch
                {
                    return Empty;
                }
            }            
        }
    }
}
