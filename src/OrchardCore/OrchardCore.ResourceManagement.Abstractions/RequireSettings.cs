using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

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
        public List<string> Dependencies { get; set; }
        public Action<ResourceDefinition> InlineDefinition { get; set; }
        public ResourcePosition Position { get; set; }

        public Dictionary<string, string> Attributes
        {
            get => _attributes ??= new Dictionary<string, string>();
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
            AppendVersion = options.AppendVersion;
        }

        public bool HasAttributes
        {
            get { return _attributes != null && _attributes.Any(a => a.Value != null); }
        }

        /// <summary>
        /// The resource will be displayed in the head of the page
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RequireSettings AtHead()
        {
            return AtLocation(ResourceLocation.Head);
        }

        /// <summary>
        /// The resource will be displayed at the foot of the page
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RequireSettings AtFoot()
        {
            return AtLocation(ResourceLocation.Foot);
        }

        /// <summary>
        /// The resource will be displayed at the specified location
        /// </summary>
        /// <param name="location">The location where the resource should be displayed</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RequireSettings AtLocation(ResourceLocation location)
        {
            // if head is specified it takes precedence since it's safer than foot
            Location = (ResourceLocation)Math.Max((int)Location, (int)location);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RequireSettings UseCulture(string cultureName)
        {
            if (!String.IsNullOrEmpty(cultureName))
            {
                Culture = cultureName;
            }
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RequireSettings UseDebugMode()
        {
            return UseDebugMode(true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RequireSettings UseDebugMode(bool debugMode)
        {
            DebugMode |= debugMode;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RequireSettings UseCdn()
        {
            return UseCdn(true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RequireSettings UseCdn(bool cdn)
        {
            CdnMode |= cdn;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RequireSettings UseCdnBaseUrl(string cdnBaseUrl)
        {
            CdnBaseUrl = cdnBaseUrl;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RequireSettings WithBasePath(string basePath)
        {
            BasePath = basePath;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RequireSettings UseCondition(string condition)
        {
            Condition ??= condition;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RequireSettings UseVersion(string version)
        {
            if (!String.IsNullOrEmpty(version))
            {
                Version = version;
            }
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RequireSettings ShouldAppendVersion(bool? appendVersion)
        {
            AppendVersion = appendVersion;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RequireSettings SetDependencies(params string[] dependencies)
        {
            Dependencies ??= new List<string>();

            Dependencies.AddRange(dependencies);

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
            _attributes ??= new Dictionary<string, string>();
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

        public RequireSettings UpdatePositionFromDependent(RequireSettings dependent)
        {
            if (dependent.Position == ResourcePosition.First && Position == ResourcePosition.Last)
            {
                throw new InvalidOperationException($"Invalid dependency position of type '{dependent.Type}' for resource '{dependent.Name}' positioned at '{dependent.Position}' depending on '{Name}' positioned at '{Position}'");
            }

            // If a 'First' resource depends on a 'ByDependency' resource, position the dependency 'First'.
            if (dependent.Position == ResourcePosition.First && Position == ResourcePosition.ByDependency)
            {
                Position = ResourcePosition.First;
            }

            return this;
        }

        public RequireSettings UpdatePositionFromDependency(RequireSettings dependency)
        {
            // If a 'ByDependency' resource depends on a 'Last' resource, position the dependent 'Last'.
            if (Position == ResourcePosition.ByDependency && dependency.Position == ResourcePosition.Last)
            {
                Position = ResourcePosition.Last;
            }

            return this;
        }

        public RequireSettings CombinePosition(RequireSettings dependent)
        {
            UpdatePositionFromDependent(dependent);
            dependent.UpdatePositionFromDependency(this);

            return this;
        }

        public RequireSettings NewAndCombine(RequireSettings other)
        {
            return new RequireSettings
            {
                Name = Name,
                Type = Type,
                Location = Location,
                Position = Position
            }
                .Combine(other)
                ;
        }

        public RequireSettings Combine(RequireSettings other)
        {
            AtLocation(Location).AtLocation(other.Location)
            .WithBasePath(BasePath).WithBasePath(other.BasePath)
            .UseCdn(CdnMode).UseCdn(other.CdnMode)
            .UseCdnBaseUrl(CdnBaseUrl).UseCdnBaseUrl(other.CdnBaseUrl)
            .UseDebugMode(DebugMode).UseDebugMode(other.DebugMode)
            .UseCulture(Culture).UseCulture(other.Culture)
            .UseCondition(Condition).UseCondition(other.Condition)
            .UseVersion(Version).UseVersion(other.Version)
            .ShouldAppendVersion(AppendVersion).ShouldAppendVersion(other.AppendVersion)
            .Define(InlineDefinition).Define(other.InlineDefinition)
            ;

            _attributes = MergeAttributes(other);
            return this;
        }
    }
}
