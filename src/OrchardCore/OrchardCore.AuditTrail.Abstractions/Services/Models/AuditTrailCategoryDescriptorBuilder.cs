using System;
using System.Collections.Generic;
using Microsoft.Extensions.Localization;

namespace OrchardCore.AuditTrail.Services.Models
{
    public class AuditTrailCategoryDescriptorBuilder<TLocalizer> : AuditTrailCategoryDescriptorBuilder where TLocalizer : class
    {
        public AuditTrailCategoryDescriptorBuilder(
            string categoryName,
            Func<IStringLocalizer, LocalizedString> localizedName
            ) : base(categoryName, typeof(IStringLocalizer<>).MakeGenericType(typeof(TLocalizer)), localizedName)
        {
        }
    }

    public class AuditTrailCategoryDescriptorBuilder
    {
        private readonly string _categoryName;

        private readonly Type _localizerType;
        private readonly Func<IServiceProvider, LocalizedString> _localizedCategoryName;
        private readonly Dictionary<string, AuditTrailEventDescriptor> _events = new Dictionary<string, AuditTrailEventDescriptor>();

        public AuditTrailCategoryDescriptorBuilder(string categoryName, Type localizerType, Func<IStringLocalizer, LocalizedString> localizedName)
        {
            _categoryName = categoryName;
            _localizerType = localizerType;
            _localizedCategoryName = (sp) => localizedName((IStringLocalizer)sp.GetService(_localizerType));
        }

        public AuditTrailCategoryDescriptorBuilder WithEvent(
            string name,
            Func<IStringLocalizer, LocalizedString> localizedName,
            Func<IStringLocalizer, LocalizedString> description,
            bool enableByDefault = false,
            bool isMandatory = false)
        {
            _events[name] = new AuditTrailEventDescriptor(
                name,
                _categoryName,
                (sp) => localizedName((IStringLocalizer)sp.GetService(_localizerType)),
                _localizedCategoryName,
                (sp) => description((IStringLocalizer)sp.GetService(_localizerType)),
                enableByDefault,
                isMandatory
            );

            return this;
        }

        public AuditTrailCategoryDescriptorBuilder RemoveEvent(string name)
        {
            _events.Remove(name);

            return this;
        }

        internal AuditTrailCategoryDescriptor Build()
            => new AuditTrailCategoryDescriptor(_categoryName, _localizedCategoryName, _events);
    }
}
