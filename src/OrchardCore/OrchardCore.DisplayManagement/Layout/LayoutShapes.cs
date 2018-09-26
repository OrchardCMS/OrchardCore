using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.DisplayManagement.Zones
{
    public class LayoutShapes : IShapeTableProvider
    {
        private readonly IHttpContextAccessor _accessor;

        public LayoutShapes(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public void Discover(ShapeTableBuilder builder)
        {
            builder
                .Describe("Layout")
                .OnCreating(creating =>
                {
                    creating.CreateAsync = () => Task.FromResult<IShape>(new ZoneHolding(() => creating.ShapeFactory.CreateAsync("Zone")));
                })
                .OnCreated(async created => 
                {
                    dynamic layout = created.Shape;

                    layout.Head = await created.ShapeFactory.CreateAsync("DocumentZone", new { ZoneName = "Head" });
                    layout.Body = await created.ShapeFactory.CreateAsync("DocumentZone", new { ZoneName = "Body" });
                    layout.Tail = await created.ShapeFactory.CreateAsync("DocumentZone", new { ZoneName = "Tail" });

                    layout.Content = await created.ShapeFactory.CreateAsync("Zone", new { ZoneName = "Content" });

                    var httpContext = _accessor.HttpContext;
                    if (httpContext != null)
                    {
                        if (AdminAttribute.IsApplied(httpContext))
                        {
                            layout.Metadata.Alternates.Add("Layout__Admin");
                        }
                        else
                        {
                            var area = httpContext.GetRouteValue("area").ToString().Replace(".", "_");
                            layout.Metadata.Alternates.Add($"Layout__url__{httpContext.Request.Path.ToUriComponent().HtmlClassify().ToLower()}");
                            layout.Metadata.Alternates.Add($"Layout__module__{area}");
                        }
                    }
                });
        }
   }
}