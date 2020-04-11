using System;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Localization.PortableObject
{
    /// <summary>
    /// Represents an <see cref="IHtmlLocalizerFactory"/> for portable objects.
    /// </summary>
    public class PortableObjectHtmlLocalizerFactory : IHtmlLocalizerFactory
    {
        private readonly IStringLocalizerFactory _stringLocalizerFactory;

        /// <summary>
        /// Creates a new instance of <see cref="PortableObjectHtmlLocalizerFactory"/>.
        /// </summary>
        /// <param name="stringLocalizerFactory">The <see cref="IStringLocalizerFactory"/>.</param>
        public PortableObjectHtmlLocalizerFactory(IStringLocalizerFactory stringLocalizerFactory)
        {
            _stringLocalizerFactory = stringLocalizerFactory;
        }

        /// <inheritdocs />
        public IHtmlLocalizer Create(Type resourceSource)
        {
            return new PortableObjectHtmlLocalizer(_stringLocalizerFactory.Create(resourceSource));
        }

        /// <inheritdocs />
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

            if (baseName.Length > index && baseName.IndexOf("Areas.", index, StringComparison.Ordinal) == index)
            {
                index += "Areas.".Length;
            }

            var relativeName = baseName.Substring(index);

            return new PortableObjectHtmlLocalizer(_stringLocalizerFactory.Create(baseName, location));
        }
    }
}
