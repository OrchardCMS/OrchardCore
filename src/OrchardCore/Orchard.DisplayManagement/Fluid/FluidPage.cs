using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Orchard.DisplayManagement.Fluid
{
    public class FluidPage : Razor.RazorPage
    {
        public static readonly string Content =
            "@inherits Orchard.DisplayManagement.Fluid.FluidPage" +
            System.Environment.NewLine + "@{ await base.ExecuteAsync(); }";

        public override async Task ExecuteAsync()
        {
            await FluidViewTemplate.RenderAsync(this);
        }

        public T GetService<T>()
        {
            return Context.RequestServices.GetService<T>();
        }

        public object GetService(Type type)
        {
            return Context.RequestServices.GetService(type);
        }

        [RazorInject]
        public IUrlHelper Url { get; private set; }
    }
}
