using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Orchard.DisplayManagement.Fluid
{
    public class FluidPage : Razor.RazorPage
    {
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
    }
}
