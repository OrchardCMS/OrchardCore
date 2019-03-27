using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Descriptors;

namespace OrchardCore.DisplayManagement.Shapes
{
    
    public abstract class PageTitleShape : IShapeAttributeProvider
    {

        IStringLocalizer T { get; }
        IHtmlLocalizer H { get; }
        [Shape]
        public IHtmlContent TimeSpan(IHtmlHelper Html, DateTime? Utc, DateTime? Origin)
        {
            // IHtmlContent f = new HtmlContentBuilder();
            //var time =  Utc.Value;
            return H["a moment ago"];
        }

        //private IPageTitleBuilder _pageTitleBuilder;
        //public IPageTitleBuilder Title
        //{
        //    get
        //    {
        //        if (_pageTitleBuilder == null)
        //        {
        //            _pageTitleBuilder = null; // HttpContext.RequestServices.GetRequiredService<IPageTitleBuilder>();
        //        }

        //        return _pageTitleBuilder;
        //    }
        //}


        //   public IHtmlContent PageTitle(IHtmlHelper Html, IHtmlContent segment,string position = "0", IHtmlContent separator = null)

        [Shape]
        public IHtmlContent PageTitle(IHtmlHelper Html, string Format)
        {
            //return Title.GenerateTitle(null);

          //  Title.AddSegment(new HtmlString(HtmlEncoder.Encode(segment)), position);
            //return Title.GenerateTitle(null);



            return Html.Raw(Html.Encode("TEST TITLE"));
        }

        //public override Task ExecuteAsync()
        //{
        //    throw new NotImplementedException();
        //}
    }
}
