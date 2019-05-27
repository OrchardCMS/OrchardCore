using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;

namespace OrchardCore.Menu
{
    public class CulturePickerShapes : IShapeTableProvider
    {
        public void Discover(ShapeTableBuilder builder)
        {
            builder.Describe("CulturePicker")
                .OnProcessing(async context =>
                {
                    var culturePicker = context.Shape;
                    var shapeFactory = context.ServiceProvider.GetRequiredService<IShapeFactory>();
                    var httpContextAccessor = context.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
                    var httpContext = httpContextAccessor.HttpContext;
                    var localizationOptions = httpContext.RequestServices.GetService<IOptions<RequestLocalizationOptions>>();

                    foreach (var culture in localizationOptions.Value.SupportedUICultures)
                    {
                        var shape = await shapeFactory.CreateAsync("CulturePickerItem", Arguments.From(new
                        {
                            Text = culture.DisplayName,
                            Value = culture.Name
                        }));

                        //culturePicker.Items.Add(shape); This is not added in @Model.Items
                    }
                });
        }
    }
}