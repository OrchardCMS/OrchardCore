using System;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Localization.PortableObject
{
    public class PortableObjectHtmlLocalizerFactory : IHtmlLocalizerFactory
    {
        private readonly IStringLocalizerFactory _stringLocalizerFactory;

        public PortableObjectHtmlLocalizerFactory(IStringLocalizerFactory stringLocalizerFactory)
        {
            _stringLocalizerFactory = stringLocalizerFactory;
        }

        public IHtmlLocalizer Create(Type resourceSource)
        {
            return new PortableObjectHtmlLocalizer(_stringLocalizerFactory.Create(resourceSource));
        }

        public IHtmlLocalizer Create(string baseName, string location)
        {
            var index = 0;
            if (baseName.StartsWith(location, StringComparison.OrdinalIgnoreCase))
            {
                index = location.Length;
            }

            if (baseName.Length > index && baseName[index] == '.')
            {
                index += 1;
            }

            if (baseName.Length > index && baseName.IndexOf("Areas.", index) == index)
            {
                index += "Areas.".Length;
            }

            var relativeName = baseName.Substring(index);

            return new PortableObjectHtmlLocalizer(_stringLocalizerFactory.Create(baseName, location));
        }
    }
}
