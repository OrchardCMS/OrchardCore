using System;
using System.Linq;
using System.Collections.Generic;

namespace OrchardCore.ResourceManagement
{
    public class RequireSettings
    {
        private Dictionary<string, string> _attributes;

        public string BasePath { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Culture { get; set; }
        public bool DebugMode { get; set; }
        public bool CdnMode { get; set; }
        public string CdnBaseUrl { get; set; }
        public ResourceLocation Location { get; set; }
        public string Condition { get; set; }
        public string Version { get; set; }
        public bool? AppendVersion { get; set; }
        public Action<ResourceDefinition> InlineDefinition { get; set; }
        public Dictionary<string, string> Attributes
        {
            get { return _attributes ?? (_attributes = new Dictionary<string, string>()); }
            private set { _attributes = value; }
        }

        public RequireSettings()
        {

        }

        public RequireSettings(ResourceManagementOptions options)
        {
            CdnMode = options.UseCdn;
            DebugMode = options.DebugMode;
            Culture = options.Culture;
            CdnBaseUrl = options.CdnBaseUrl;
        }

        public bool HasAttributes
        {
            get { return _attributes != null && _attributes.Any(a => a.Value != null); }
        }
        
        /// <summary>
        /// The resource will be displayed in the head of the page
        /// </summary>
        public RequireSettings AtHead()
        {
            return AtLocation(ResourceLocation.Head);
        }

        /// <summary>
        /// The resource will be displayed at the foot of the page
        /// </summary>
        /// <returns></returns>
        public RequireSettings AtFoot()
        {
            return AtLocation(ResourceLocation.Foot);
        }

        /// <summary>
        /// The resource will be displayed at the specified location
        /// </summary>
        /// <param name="location">The location where the resource should be displayed</param>
        public RequireSettings AtLocation(ResourceLocation location)
        {
            // if head is specified it takes precedence since it's safer than foot
            Location = (ResourceLocation)Math.Max((int)Location, (int)location);
            return this;
        }

        public RequireSettings UseCulture(string cultureName)
        {
            if (!String.IsNullOrEmpty(cultureName))
            {
                Culture = cultureName;
            }
            return this;
        }

        public RequireSettings UseDebugMode()
        {
            return UseDebugMode(true);
        }

        public RequireSettings UseDebugMode(bool debugMode)
        {
            DebugMode |= debugMode;
            return this;
        }

        public RequireSettings UseCdn()
        {
            return UseCdn(true);
        }

        public RequireSettings UseCdn(bool cdn)
        {
            CdnMode |= cdn;
            return this;
        }

        public RequireSettings UseCdnBaseUrl(string cdnBaseUrl)
        {
            CdnBaseUrl = cdnBaseUrl;
            return this;
        }

        public RequireSettings WithBasePath(string basePath)
        {
            BasePath = basePath;
            return this;
        }

        public RequireSettings UseCondition(string condition)
        {
            Condition = Condition ?? condition;
            return this;
        }

        public RequireSettings UseVersion(string version)
        {
            if (!String.IsNullOrEmpty(version))
            {
                Version = version;
            }
            return this;
        }

        public RequireSettings SetAppendVersion(bool? appendVersion)
        {
            AppendVersion = appendVersion;
            return this;
        }

        public RequireSettings Define(Action<ResourceDefinition> resourceDefinition)
        {
            if (resourceDefinition != null)
            {
                var previous = InlineDefinition;
                if (previous != null)
                {
                    InlineDefinition = r =>
                    {
                        previous(r);
                        resourceDefinition(r);
                    };
                }
                else
                {
                    InlineDefinition = resourceDefinition;
                }
            }
            return this;
        }

        public RequireSettings SetAttribute(string name, string value)
        {
            if (_attributes == null)
            {
                _attributes = new Dictionary<string, string>();
            }
            _attributes[name] = value;
            return this;
        }

        private Dictionary<string, string> MergeAttributes(RequireSettings other)
        {
            // efficiently merge the two dictionaries, taking into account that one or both may not exist
            // and that attributes in 'other' should overridde attributes in this, even if the value is null.
            if (_attributes == null)
            {
                return other._attributes == null ? null : new Dictionary<string, string>(other._attributes);
            }
            if (other._attributes == null)
            {
                return new Dictionary<string, string>(_attributes);
            }
            var mergedAttributes = new Dictionary<string, string>(_attributes);
            foreach (var pair in other._attributes)
            {
                mergedAttributes[pair.Key] = pair.Value;
            }
            return mergedAttributes;
        }

        public RequireSettings Combine(RequireSettings other)
        {
            var settings = (new RequireSettings
            {
                Name = Name,
                Type = Type,
            }).AtLocation(Location).AtLocation(other.Location)
                .WithBasePath(BasePath).WithBasePath(other.BasePath)
                .UseCdn(CdnMode).UseCdn(other.CdnMode)
                .UseCdnBaseUrl(CdnBaseUrl).UseCdnBaseUrl(other.CdnBaseUrl)
                .UseDebugMode(DebugMode).UseDebugMode(other.DebugMode)
                .UseCulture(Culture).UseCulture(other.Culture)
                .UseCondition(Condition).UseCondition(other.Condition)
                .UseVersion(Version).UseVersion(other.Version)
                .SetAppendVersion(AppendVersion).SetAppendVersion(other.AppendVersion)
                .Define(InlineDefinition).Define(other.InlineDefinition);
            settings._attributes = MergeAttributes(other);
            return settings;
        }
    }
}
