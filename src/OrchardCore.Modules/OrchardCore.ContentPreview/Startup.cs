using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentPreview.Controllers;
using OrchardCore.ContentPreview.Drivers;
using OrchardCore.ContentPreview.Models;
using OrchardCore.ContentPreview.Settings;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Modules;
using OrchardCore.Security.Permissions;

namespace OrchardCore.ContentPreview
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IContentDisplayDriver, ContentPreviewDriver>();
            services.AddScoped<IPermissionProvider, Permissions>();

            // Preview Part
            services.AddContentPart<PreviewPart>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, PreviewPartSettingsDisplayDriver>();
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder endpoints, IServiceProvider serviceProvider)
        {
            app.Use((context, next) =>
            {
                if (context.Request.Method == "POST" && context.Request.Path == "/OrchardCore.ContentPreview/Preview/Render")
                {
                    return HandlePreview(context, next);
                }
                else
                {
                    return next();
                }
            });
        }

        private static async Task HandlePreview(HttpContext context, Func<Task> next)
        {
            var _authorizationService = context.RequestServices.GetRequiredService<IAuthorizationService>();
            var user = context.User;
            var _contentManager = context.RequestServices.GetRequiredService<IContentManager>();
            var _contentManagerSession = context.RequestServices.GetRequiredService<IContentManagerSession>();
            var _contentItemDisplayManager = context.RequestServices.GetRequiredService<IContentItemDisplayManager>();
            var _htmlDisplay = context.RequestServices.GetRequiredService<IHtmlDisplay>();
            var _clock = context.RequestServices.GetRequiredService<IClock>();
            var _modelUpdater = context.RequestServices.GetRequiredService<IUpdateModelAccessor>().ModelUpdater;
            var _displayHelper = context.RequestServices.GetRequiredService<IDisplayHelper>();

            if (!await _authorizationService.AuthorizeAsync(user, Permissions.ContentPreview))
            {
                return;
                // return Unauthorized();
            }

            var contentItemType = context.Request.Form["ContentItemType"];
            var contentItem = await _contentManager.NewAsync(contentItemType);

            // Assign the ids from the currently edited item so that validation thinks
            // it's working on the same item. For instance if drivers are checking name unicity
            // they need to think this is the same existing item (AutoroutePart).

            var contentItemId = context.Request.Form["PreviewContentItemId"];
            var contentItemVersionId = context.Request.Form["PreviewContentItemVersionId"];
            int.TryParse(context.Request.Form["PreviewId"], out var contentId);

            contentItem.Id = contentId;
            contentItem.ContentItemId = contentItemId;
            contentItem.ContentItemVersionId = contentItemVersionId;
            contentItem.CreatedUtc = _clock.UtcNow;
            contentItem.ModifiedUtc = _clock.UtcNow;
            contentItem.PublishedUtc = _clock.UtcNow;


            var controller = new PreviewController();
            controller.ControllerContext = new ControllerContext { HttpContext = context };
            var modelUpdater = new ControllerModelUpdater(controller);

            // TODO: we should probably get this value from the main editor as it might impact validators
            var model = await _contentItemDisplayManager.UpdateEditorAsync(contentItem, modelUpdater, true);

            if (!_modelUpdater.ModelState.IsValid)
            {
                var errors = new List<string>();
                foreach (var modelState in _modelUpdater.ModelState)
                {
                    //for (var i = 0; i < modelState.Errors.Count; i++)
                    //{
                    //    var modelError = modelState.Errors[i];
                    //    var errorText = ValidationHelpers.GetModelErrorMessageOrDefault(modelError);
                    foreach (var error in modelState.Value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                    //}
                }

                context.Response.StatusCode = 500;
                await context.Response.WriteAsync(" { errors = [ 'some errors here' ] } ");

                return;
                //return StatusCode(500, new { errors = errors });
            }

            var previewAspect = await _contentManager.PopulateAspectAsync(contentItem, new PreviewAspect());

            if (!String.IsNullOrEmpty(previewAspect.PreviewUrl))
            {
                // The PreviewPart is configured, we need to set the fake content item
                _contentManagerSession.Store(contentItem);
                context.Request.Path = previewAspect.PreviewUrl;
            }

            await next();
            //model = await _contentItemDisplayManager.BuildDisplayAsync(contentItem, _modelUpdater, "Detail");
            //var displayContext = new DisplayContext
            //{
            //    DisplayAsync = _displayHelper,
            //    ServiceProvider = context.RequestServices,
            //    Value = model
            //};

            //var htmlContent = await _htmlDisplay.ExecuteAsync(displayContext);
            //await context.Response.WriteAsync(htmlContent.ToString());
        }
    }
}
