//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Text.Encodings.Web;
//using System.Threading.Tasks;
//using Fluid;
//using Fluid.Ast;
//using Microsoft.Extensions.DependencyInjection;
//using OrchardCore.Liquid;
//using OrchardCore.ResourceManagement;

//namespace OrchardCore.Resources.Liquid
//{
//    public class MetaTag
//    {
//        public static async ValueTask<Completion> WriteToAsync(List<FilterArgument> argumentsList, TextWriter writer, TextEncoder encoder, TemplateContext context)
//        {
//            var services = ((LiquidTemplateContext)context).Services;
//            var resourceManager = services.GetRequiredService<IResourceManager>();

//            var arguments = new FilterArguments();

//            foreach (var argument in argumentsList)
//            {
//                arguments.Add(argument.Name, await argument.Expression.EvaluateAsync(context));
//            }

//            var metaEntry = new MetaEntry(Name, Property, Content, HttpEquiv, Charset);

//            foreach (var attribute in output.Attributes)
//            {
//                if (String.Equals(attribute.Name, "name", StringComparison.OrdinalIgnoreCase) || String.Equals(attribute.Name, "property", StringComparison.OrdinalIgnoreCase))
//                {
//                    continue;
//                }

//                metaEntry.SetAttribute(attribute.Name, attribute.Value.ToString());
//            }

//            _resourceManager.AppendMeta(metaEntry, Separator ?? ", ");

//            return Completion.Normal;
//        }
//    }
//}
