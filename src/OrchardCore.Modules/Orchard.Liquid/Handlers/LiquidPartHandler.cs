using System;
using System.Threading.Tasks;
using Fluid;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.Models;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Fluid;
using Orchard.DisplayManagement.Implementation;
using Orchard.Liquid.Model;

namespace Orchard.Liquid.Handlers
{
    public class LiquidPartHandler : ContentPartHandler<LiquidPart>
    {
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly IDisplayHelperFactory _displayHelperFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IServiceProvider _services;

        public LiquidPartHandler(
            ILiquidTemplateManager liquidTemplateManager,
            IDisplayHelperFactory displayHelperFactory,
            IHttpContextAccessor httpContextAccessor,
            IServiceProvider services)
        {
            _liquidTemplateManager = liquidTemplateManager;
            _displayHelperFactory = displayHelperFactory;
            _httpContextAccessor = httpContextAccessor;
            _services = services;
        }

        public override void GetContentItemAspect(ContentItemAspectContext context, LiquidPart part)
        {
            context.For<BodyAspect>(bodyAspect =>
            {
                if (FluidViewTemplate.TryParse(part.Liquid, out var template, out var errors))
                {
                    var viewContext = new ViewContext()
                    {
                        HttpContext = _httpContextAccessor.HttpContext,
                        ExecutingFilePath = "/Views/Foo.cshtml",
                        View = new FakeView()
                    };

                    var displayHelper = _displayHelperFactory.CreateHelper(viewContext);

                    var model = new
                    {
                        ContentItem = part.ContentItem
                    };

                    var ctx = new TemplateContext();

                    ctx.Contextualize(new DisplayContext()
                    {
                        ServiceProvider = _services,
                        DisplayAsync = displayHelper,
                        ViewContext = viewContext,
                        Value = model
                    });

                    var htmlContent = _liquidTemplateManager.RenderAsync(part.Liquid, ctx).GetAwaiter().GetResult();
                    bodyAspect.Body = new HtmlString(htmlContent);
                }
                else
                {
                    bodyAspect.Body = HtmlString.Empty;
                }
            });
        }

        internal class FakeView : IView
        {
            public string Path => "/Views/Foo.cshtml";

            public Task RenderAsync(ViewContext context)
            {
                throw new NotImplementedException();
            }
        }
    }
}