using Microsoft.AspNetCore.Http;

namespace OrchardCore.DisplayManagement.ModelBinding
{
    public class LocalModelBinderAccessor : IUpdateModelAccessor
    {
        private readonly static object Key = typeof(LocalModelBinderAccessor);
        private readonly HttpContext _httpContext;

        public LocalModelBinderAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContext = httpContextAccessor.HttpContext;
        }

        public IUpdateModel ModelUpdater
        {
            get
            {
                var updateModel = _httpContext.Items[Key] as IUpdateModel;
                return updateModel ?? new NullModelUpdater();
            }

            set { _httpContext.Items[Key] = value; }
        }
    }
}
