using Microsoft.AspNetCore.Http;

namespace OrchardCore.DisplayManagement.ModelBinding
{
    public class LocalModelBinderAccessor : IUpdateModelAccessor
    {
        private static readonly object _key = typeof(LocalModelBinderAccessor);
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LocalModelBinderAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public IUpdateModel ModelUpdater
        {
            get
            {
                var updateModel = _httpContextAccessor.HttpContext.Items[_key] as IUpdateModel;
                return updateModel ?? new NullModelUpdater();
            }

            set { _httpContextAccessor.HttpContext.Items[_key] = value; }
        }
    }
}
