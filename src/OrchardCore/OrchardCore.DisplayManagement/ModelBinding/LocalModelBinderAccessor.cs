using Microsoft.AspNetCore.Http;

namespace OrchardCore.DisplayManagement.ModelBinding
{
    public class LocalModelBinderAccessor : IUpdateModelAccessor
    {
        private readonly static object Key = typeof(LocalModelBinderAccessor);
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LocalModelBinderAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public IUpdateModel ModelUpdater
        {
            get
            {
                var updateModel = _httpContextAccessor.HttpContext.Items[Key] as IUpdateModel;
                return updateModel ?? new NullModelUpdater();
            }

            set { _httpContextAccessor.HttpContext.Items[Key] = value; }
        }
    }
}
