using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.DisplayManagement.Shapes
{
    public class PageTitleShape : IShapeAttributeProvider
    {

        IStringLocalizer T { get; }
        IHtmlLocalizer H { get; }

        [Shape]
        public IHtmlContent TimeSpan(IHtmlHelper Html, DateTime? Utc, DateTime? Origin)
        {

        }
}
