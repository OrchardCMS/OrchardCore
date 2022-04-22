using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Drivers;
using OrchardCore.Security.Options;
using OrchardCore.Security.Permissions;
using OrchardCore.Security.Services;
using OrchardCore.Settings;

namespace OrchardCore.Security
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IPermissionProvider, SecurityPermissions>();
            services.AddScoped<IDisplayDriver<ISite>, SecuritySettingsDisplayDriver>();
            services.AddScoped<INavigationProvider, AdminMenu>();

            services.AddSingleton<ISecurityService, SecurityService>();

            services.AddTransient<IConfigureOptions<SecuritySettings>, SecuritySettingsConfiguration>();
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var securityOptions = serviceProvider.GetRequiredService<IOptions<SecuritySettings>>().Value;

            builder.UseSecurityHeaders(config =>
            {
                var permissionsPolicyOptions = GetPermissionsPolicyOptions(securityOptions);

                config.AddContentTypeOptions();
                config.AddFrameOptions(new FrameOptionsOptions { Value = securityOptions.FrameOptions });
                config.AddReferrerPolicy(new ReferrerPolicyOptions { Value = securityOptions.ReferrerPolicy });
                config.AddPermissionsPolicy(permissionsPolicyOptions);
            });
        }

        private static PermissionsPolicyOptions GetPermissionsPolicyOptions(SecuritySettings securitySettings)
        {
            var options = new PermissionsPolicyOptions();
            var builder = new PermissionsPolicyOptionsBuilder(options);
            var origin = securitySettings.PermissionsPolicyOrigin;

            if (securitySettings.PermissionsPolicy == null)
            {
                return options;
            }

            foreach (var policy in securitySettings.PermissionsPolicy)
            {
                if (policy == PermissionsPolicyValue.Accelerometer)
                {
                    builder.AllowAccelerometer(origin);
                }
                else if (policy == PermissionsPolicyValue.AmbientLightSensor)
                {
                    builder.AllowAmbientLightSensor(origin);
                }
                else if (policy == PermissionsPolicyValue.Autoplay)
                {
                    builder.AllowAutoplay(origin);
                }
                else if (policy == PermissionsPolicyValue.Camera)
                {
                    builder.AllowCamera(origin);
                }
                else if (policy == PermissionsPolicyValue.EncryptedMedia)
                {
                    builder.AllowEncryptedMedia(origin);
                }
                else if (policy == PermissionsPolicyValue.FullScreen)
                {
                    builder.AllowFullScreen(origin);
                }
                else if (policy == PermissionsPolicyValue.Geolocation)
                {
                    builder.AllowGeolocation(origin);
                }
                else if (policy == PermissionsPolicyValue.Gyroscope)
                {
                    builder.AllowGyroscope(origin);
                }
                else if (policy == PermissionsPolicyValue.Magnetometer)
                {
                    builder.AllowMagnetometer(origin);
                }
                else if (policy == PermissionsPolicyValue.Microphone)
                {
                    builder.AllowMicrophone(origin);
                }
                else if (policy == PermissionsPolicyValue.Midi)
                {
                    builder.AllowMidi(origin);
                }
                else if (policy == PermissionsPolicyValue.Notifications)
                {
                    builder.AllowNotifications(origin);
                }
                else if (policy == PermissionsPolicyValue.Payment)
                {
                    builder.AllowPayment(origin);
                }
                else if (policy == PermissionsPolicyValue.PictureInPicture)
                {
                    builder.AllowPictureInPicture(origin);
                }
                else if (policy == PermissionsPolicyValue.Push)
                {
                    builder.AllowPush(origin);
                }
                else if (policy == PermissionsPolicyValue.Notifications)
                {
                    builder.AllowNotifications(origin);
                }
                else if (policy == PermissionsPolicyValue.Speaker)
                {
                    builder.AllowSpeaker(origin);
                }
                else if (policy == PermissionsPolicyValue.SyncXhr)
                {
                    builder.AllowSyncXhr(origin);
                }
                else if (policy == PermissionsPolicyValue.Usb)
                {
                    builder.AllowUsb(origin);
                }
                else if (policy == PermissionsPolicyValue.Vibrate)
                {
                    builder.AllowVibrate(origin);
                }
                else if (policy == PermissionsPolicyValue.VR)
                {
                    builder.AllowVR(origin);
                }
            }

            return options;
        }
    }
}
