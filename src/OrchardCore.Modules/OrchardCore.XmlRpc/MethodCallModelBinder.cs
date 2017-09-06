using System;
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
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            if (bindingContext.ModelType == typeof(XRpcMethodCall))
            {
                var mapper = bindingContext.HttpContext.RequestServices.GetRequiredService<IXmlRpcReader>();
                var element = XElement.Load(bindingContext.HttpContext.Request.Body);

                bindingContext.Result = ModelBindingResult.Success(mapper.MapToMethodCall(element));
            }

            return Task.CompletedTask;
        }
    }
}
