using System;
using System.Text;

namespace OrchardCore.Security.Options
{
    public class ContentSecurityPolicyOptionsBuilder
    {
        private readonly ContentSecurityPolicyOptions _options;

        public ContentSecurityPolicyOptionsBuilder(ContentSecurityPolicyOptions options)
            => _options = options ?? throw new ArgumentNullException(nameof(options));

        public ContentSecurityPolicyOptionsBuilder AllowBaseUri(string source, params string[] allowedSources)
        {
            _options.BaseUri.Source = source;
            _options.BaseUri.AllowedSources = allowedSources;

            return this;
        }

        public ContentSecurityPolicyOptionsBuilder AllowChildSource(string source, params string[] allowedSources)
        {
            _options.ChildSource.Source = source;
            _options.ChildSource.AllowedSources = allowedSources;

            return this;
        }

        public ContentSecurityPolicyOptionsBuilder AllowConnectSource(string source, params string[] allowedSources)
        {
            _options.ConnectSource.Source = source;
            _options.ConnectSource.AllowedSources = allowedSources;

            return this;
        }

        public ContentSecurityPolicyOptionsBuilder AllowDefaultSource(string source, params string[] allowedSources)
        {
            _options.DefaultSource.Source = source;
            _options.DefaultSource.AllowedSources = allowedSources;

            return this;
        }

        public ContentSecurityPolicyOptionsBuilder AllowFontSource(string source, params string[] allowedSources)
        {
            _options.FontSource.Source = source;
            _options.FontSource.AllowedSources = allowedSources;

            return this;
        }

        public ContentSecurityPolicyOptionsBuilder AllowFormAction(string source, params string[] allowedSources)
        {
            _options.FormAction.Source = source;
            _options.FormAction.AllowedSources = allowedSources;

            return this;
        }

        public ContentSecurityPolicyOptionsBuilder AllowFrameAncestors(string source, params string[] allowedSources)
        {
            _options.FrameAncestors.Source = source;
            _options.FrameAncestors.AllowedSources = allowedSources;

            return this;
        }

        public ContentSecurityPolicyOptionsBuilder AllowFrameSource(string source, params string[] allowedSources)
        {
            _options.FrameSource.Source = source;
            _options.FrameSource.AllowedSources = allowedSources;

            return this;
        }

        public ContentSecurityPolicyOptionsBuilder AllowImageSource(string source, params string[] allowedSources)
        {
            _options.ImageSource.Source = source;
            _options.ImageSource.AllowedSources = allowedSources;

            return this;
        }

        public ContentSecurityPolicyOptionsBuilder AllowMediaSource(string source, params string[] allowedSources)
        {
            _options.MediaSource.Source = source;
            _options.MediaSource.AllowedSources = allowedSources;

            return this;
        }

        public ContentSecurityPolicyOptionsBuilder AllowManifestSource(string source, params string[] allowedSources)
        {
            _options.ManifestSource.Source = source;
            _options.ManifestSource.AllowedSources = allowedSources;

            return this;
        }

        public ContentSecurityPolicyOptionsBuilder AllowObjectSource(string source, params string[] allowedSources)
        {
            _options.ObjectSource.Source = source;
            _options.ObjectSource.AllowedSources = allowedSources;

            return this;
        }

        public ContentSecurityPolicyOptionsBuilder AllowSandbox(
            bool allowDownloads = false,
            bool allowForms = false,
            bool allowModals = false,
            bool allowOrientationLock = false,
            bool allowPointerLock = false,
            bool allowPopup = false,
            bool allowPopupsToEscapeSandbox = false,
            bool allowPresentation = false,
            bool allowSameSource = false,
            bool allowScripts = false,
            bool allowsStorageAccessByUserActivation = false,
            bool allowTopNavigation = false,
            bool allowTopNavigationByUseActivation = false)
        {
            var value = new StringBuilder();
            if (allowDownloads)
            {
                value
                    .Append(' ')
                    .Append(SandboxContentSecurityPolicyValue.AllowDownloads);
            }

            if (allowForms)
            {
                value
                    .Append(' ')
                    .Append(SandboxContentSecurityPolicyValue.AllowForms);
            }

            if (allowModals)
            {
                value
                    .Append(' ')
                    .Append(SandboxContentSecurityPolicyValue.AllowModals);
            }

            if (allowOrientationLock)
            {
                value
                    .Append(' ')
                    .Append(SandboxContentSecurityPolicyValue.AllowOrientationLock);
            }

            if (allowPointerLock)
            {
                value
                    .Append(' ')
                    .Append(SandboxContentSecurityPolicyValue.AllowPointerLock);
            }

            if (allowPopup)
            {
                value
                    .Append(' ')
                    .Append(SandboxContentSecurityPolicyValue.AllowPopup);
            }

            if (allowPopupsToEscapeSandbox)
            {
                value
                    .Append(' ')
                    .Append(SandboxContentSecurityPolicyValue.AllowPopupsToEscapeSandbox);
            }

            if (allowPresentation)
            {
                value
                    .Append(' ')
                    .Append(SandboxContentSecurityPolicyValue.AllowPresentation);
            }

            if (allowSameSource)
            {
                value
                    .Append(' ')
                    .Append(SandboxContentSecurityPolicyValue.AllowSameOrigin);
            }

            if (allowScripts)
            {
                value
                    .Append(' ')
                    .Append(SandboxContentSecurityPolicyValue.AllowScripts);
            }

            if (allowsStorageAccessByUserActivation)
            {
                value
                    .Append(' ')
                    .Append(SandboxContentSecurityPolicyValue.AllowsStorageAccessByUserActivation);
            }

            if (allowTopNavigation)
            {
                value
                    .Append(' ')
                    .Append(SandboxContentSecurityPolicyValue.AllowTopNavigation);
            }

            if (allowTopNavigationByUseActivation)
            {
                value
                    .Append(' ')
                    .Append(SandboxContentSecurityPolicyValue.AllowTopNavigationByUseActivation);
            }

            _options.Sandbox.Value = value.ToString();

            return this;
        }

        public ContentSecurityPolicyOptionsBuilder AllowScriptSource(string source, params string[] allowedSources)
        {
            _options.ScriptSource.Source = source;
            _options.ScriptSource.AllowedSources = allowedSources;

            return this;
        }

        public ContentSecurityPolicyOptionsBuilder AllowStyleSource(string source, params string[] allowedSources)
        {
            _options.StyleSource.Source = source;
            _options.StyleSource.AllowedSources = allowedSources;

            return this;
        }
    }
}
