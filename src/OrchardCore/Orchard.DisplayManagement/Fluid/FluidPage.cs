using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Orchard.DisplayManagement.Fluid
{
    public class FluidPage : Razor.RazorPage<dynamic>
    {
        public override async Task ExecuteAsync()
        {
            await FluidViewTemplate.RenderAsync(this);
        }

        public IServiceProvider ServiceProvider
        {
            get
            {
                return Context.RequestServices;
            }
        }

        public T GetService<T>()
        {
            return ServiceProvider.GetService<T>();
        }

        public IUrlHelper Url
        {
            get
            {
                return GetService<IUrlHelperFactory>().GetUrlHelper(ViewContext);
            }
        }
    }
}
