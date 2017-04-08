using System;
using System.Collections.Generic;
using System.Text;

namespace Orchard.Tokens.Content.Services
{
    public interface ISlugService
    {
        /// <summary>
        /// Transforms specified text to the form suitable for URL slugs
        /// </summary>
        /// <param name="text">the text to transform</param>
        /// <returns>the slug created from the input text</returns>
        string Slugify(string text);
    }
}
