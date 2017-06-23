using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Fluid;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Liquid;

namespace Orchard.DisplayManagement.Fluid
{
    public class FluidView : Razor.RazorPage<dynamic>
    {
        public static readonly string ViewExtension = ".fluid";

        private IServiceProvider ServiceProvider => Context.RequestServices;

        static FluidView()
        {
            TemplateContext.GlobalMemberAccessStrategy.Register(typeof(ViewContext));
        }

        public override async Task ExecuteAsync()
        {
            var environment = ServiceProvider.GetRequiredService<IHostingEnvironment>();

            var source = File.ReadAllText(System.IO.Path.Combine(environment.ContentRootPath,
                ViewContext.ExecutingFilePath.Replace(RazorViewEngine.ViewExtension, ViewExtension)
                .TrimStart('/')));

            if (FluidViewTemplate.TryParse(source, out var template, out var errors))
            {
                var context = new TemplateContext();

                context.MemberAccessStrategy.Register(Context.GetType());
                context.MemberAccessStrategy.Register(Context.Request.GetType());

                ViewBag.Title = "This is a title";

                context.LocalScope.SetValue("Context", Context);
                context.LocalScope.SetValue("ViewData", ViewData);
                context.LocalScope.SetValue("ViewContext", ViewContext);

                if (Model != null)
                {
                    context.LocalScope.SetValue("Model", Model);
                    context.LocalScope.SetValue("ModelState", ViewContext.ModelState);
                    context.MemberAccessStrategy.Register(((object)Model).GetType());
                }

                var urlHelperFactory = ServiceProvider.GetService<IUrlHelperFactory>();
                context.AmbientValues.Add("UrlHelper", urlHelperFactory.GetUrlHelper(ViewContext));
                context.AmbientValues.Add("FluidView", this);

                var handlers = ServiceProvider.GetService<IEnumerable<ITemplateContextHandler>>();

                foreach (var handler in handlers)
                {
                    handler.OnTemplateProcessing(context);
                }

                WriteLiteral(await template.RenderAsync(context));
            }
        }
    }
}
