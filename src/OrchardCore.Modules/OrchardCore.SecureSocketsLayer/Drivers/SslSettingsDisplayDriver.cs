using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities.DisplayManagement;
using OrchardCore.SecureSocketsLayer.Models;
using OrchardCore.SecureSocketsLayer.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.SecureSocketsLayer.Drivers {
    public class SslSettingsDisplayDriver : SectionDisplayDriver<ISite, SslSettings>
    {
        private const int DefaultSslPort = 443;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INotifier _notifier;

        private const string SettingsGroupId = "ssl";

        public SslSettingsDisplayDriver(IHttpContextAccessor httpContextAccessor,
            INotifier notifier,
            IHtmlLocalizer<SslSettingsDisplayDriver> h)
        {
            _httpContextAccessor = httpContextAccessor;
            _notifier = notifier;
            H = h;
        }
        IHtmlLocalizer H { get; set; }

        public override IDisplayResult Edit(SslSettings settings, BuildEditorContext context)
        {
            var isHttpsRequest = _httpContextAccessor.HttpContext.Request.IsHttps;

            if (!isHttpsRequest)
                _notifier.Warning(H["Enabling HTTPS over HTTP has been disabled."]);

            return Shape<SslSettingsViewModel>("SecureSocketsLayerSettings_Edit", model =>
            {
                model.IsHttpsRequest = isHttpsRequest;
                model.RequireHttps = settings.RequireHttps;
                model.RequireHttpsPermanent = settings.RequireHttpsPermanent;
                model.SslPort = settings.SslPort ??
                                (_httpContextAccessor.HttpContext.Request.Host.Port == DefaultSslPort
                                    ? null
                                    : _httpContextAccessor.HttpContext.Request.Host.Port);
            }).Location("Content:2").OnGroup(SettingsGroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(SslSettings settings, IUpdateModel updater, string groupId)
        {
            if (groupId == SettingsGroupId)
            {
                var model = new SslSettingsViewModel();

                await updater.TryUpdateModelAsync(model, Prefix);

                settings.RequireHttps = model.RequireHttps;
                settings.RequireHttpsPermanent = model.RequireHttpsPermanent;
                settings.SslPort = model.SslPort;
            }

            return Edit(settings);
        }
    }
}
