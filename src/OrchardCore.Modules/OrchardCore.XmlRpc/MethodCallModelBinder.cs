using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.XmlRpc.Models;
using OrchardCore.XmlRpc.Services;

namespace OrchardCore.XmlRpc
{
    public class MethodCallModelBinder : IModelBinder
    {
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            if (bindingContext.ModelType == typeof(XRpcMethodCall))
            {
                var mapper = bindingContext.HttpContext.RequestServices.GetRequiredService<IXmlRpcReader>();
                var bodyTextContent = String.Empty;
                // For some reasons we can't use XElement.LoadAsync. This method fails on big request body. I tested this with a picture with 2 MB of size.
                // This code is maybe bad. Because if someone send a very big XML to this endpoint the server will die a out of memory exception.
                using (StreamReader reader = new StreamReader(bindingContext.HttpContext.Request.Body, Encoding.UTF8, true, 1024, true))
                {
                    bodyTextContent = await reader.ReadToEndAsync();
                }
                var element = XElement.Parse(bodyTextContent);

                bindingContext.Result = ModelBindingResult.Success(mapper.MapToMethodCall(element));
            }
        }
    }
}
