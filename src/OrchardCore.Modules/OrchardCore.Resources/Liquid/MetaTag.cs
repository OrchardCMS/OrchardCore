using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Liquid;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Resources.Liquid
{
    public class MetaTag
    {
        public static async ValueTask<Completion> WriteToAsync(List<FilterArgument> argumentsList, TextWriter _1, TextEncoder _2, TemplateContext context)
        {
            var services = ((LiquidTemplateContext)context).Services;
            var resourceManager = services.GetRequiredService<IResourceManager>();

            string name = null;
            string property = null;
            string content = null;
            string httpEquiv = null;
            string charset = null;
            string separator = null;

            Dictionary<string, string> customAttributes = null;

            foreach (var argument in argumentsList)
            {
                switch (argument.Name)
                {
                    case "name": name = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;
                    case "property": property = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;
                    case "content": content = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;
                    case "http_equiv": httpEquiv = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;
                    case "charset": charset = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;
                    case "separator": separator = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;
                    default: (customAttributes ??= new Dictionary<string, string>())[argument.Name] = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;
                }
            }

            var metaEntry = new MetaEntry(name, property, content, httpEquiv, charset);

            if (customAttributes != null)
            {
                foreach (var attribute in customAttributes)
                {
                    metaEntry.SetAttribute(attribute.Key, attribute.Value);
                }
            }

            resourceManager.AppendMeta(metaEntry, separator ?? ", ");

            return Completion.Normal;
        }
    }
}
