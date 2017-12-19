using System.Threading.Tasks;
using JsonApiFramework.JsonApi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Apis.JsonApi;

namespace OrchardCore.Setup.JsonApi
{
    public class SetupJsonApiResultProvider : JsonApiResultProvider
    {
        private readonly static JsonApiVersion Version = JsonApiVersion.Version11;

        public override Task<Document> Build(IUrlHelper urlHelper, object actionValue)
        {
            var viewResult = actionValue as ViewResult;

            if (viewResult != null &&
                !viewResult.ViewData.ModelState.IsValid)
            {
                return BuildErrorDocument(viewResult.ViewData.ModelState);
            }

            var redirectReuslt = actionValue as RedirectResult;

            if (redirectReuslt != null)
            {
                var document = new ResourceDocument
                {
                    JsonApiVersion = Version,
                    Links = new Links {
                        {
                            Keywords.Self,
                            redirectReuslt.Url
                        }
                    }
                };
                return Task.FromResult<Document>(document);
            }

            return Task.FromResult<Document>(null);
        }

        private Task<Document> BuildErrorDocument(ModelStateDictionary modelStateDictionary)
        {
            var document = new ErrorsDocument {
                JsonApiVersion = Version
            };

            foreach (var modelState in modelStateDictionary.Values)
            {
                foreach (var error in modelState.Errors)
                {
                    document.AddError(new Error
                    {
                        Title = error.ErrorMessage,
                        Detail = error.Exception.ToString()
                    });
                }
            }

            return Task.FromResult<Document>(document);
        }
    }
}
