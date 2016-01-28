using Microsoft.AspNet.Html.Abstractions;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Mvc.Routing;
using Microsoft.AspNet.Mvc.ViewFeatures;
using Microsoft.AspNet.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.WebEncoders;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Core.Navigation
{
    public class PagerShapes : IShapeTableProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PagerShapes(
            IHttpContextAccessor httpContextAccessor,
            IStringLocalizer<PagerShapes> localizer)
        {
            _httpContextAccessor = httpContextAccessor;

            T = localizer;
        }

        public IStringLocalizer T { get; set; }

        public void Discover(ShapeTableBuilder builder)
        {
            builder.Describe("Pager")
                .OnDisplaying(displaying =>
                {
                    var pager = displaying.Shape;
                    string pagerId = pager.PagerId;
                    if (!String.IsNullOrWhiteSpace(pagerId))
                        displaying.Shape.Metadata.Alternates.Add("Pager__" + EncodeAlternateElement(pagerId));
                });

            builder.Describe("Pager_Gap")
                .OnDisplaying(displaying =>
                {
                    var pager = displaying.Shape.Pager;
                    string pagerId = pager.PagerId;
                    if (!String.IsNullOrWhiteSpace(pagerId))
                        pager.Metadata.Alternates.Add("Pager_Gap__" + EncodeAlternateElement(pagerId));
                });

            builder.Describe("Pager_First")
                .OnDisplaying(displaying =>
                {
                    var pager = displaying.Shape.Pager;
                    string pagerId = pager.PagerId;
                    if (!String.IsNullOrWhiteSpace(pagerId))
                        displaying.Shape.Metadata.Alternates.Add("Pager_First__" + EncodeAlternateElement(pagerId));
                });

            builder.Describe("Pager_Previous")
                .OnDisplaying(displaying =>
                {
                    var pager = displaying.Shape.Pager;
                    string pagerId = pager.PagerId;
                    if (!String.IsNullOrWhiteSpace(pagerId))
                        displaying.Shape.Metadata.Alternates.Add("Pager_Previous__" + EncodeAlternateElement(pagerId));
                });

            builder.Describe("Pager_Next")
                .OnDisplaying(displaying =>
                {
                    var pager = displaying.Shape.Pager;
                    string pagerId = pager.PagerId;
                    if (!String.IsNullOrWhiteSpace(pagerId))
                        displaying.Shape.Metadata.Alternates.Add("Pager_Next__" + EncodeAlternateElement(pagerId));
                });

            builder.Describe("Pager_Last")
                .OnDisplaying(displaying =>
                {
                    var pager = displaying.Shape.Pager;
                    string pagerId = pager.PagerId;
                    if (!String.IsNullOrWhiteSpace(pagerId))
                        displaying.Shape.Metadata.Alternates.Add("Pager_Last__" + EncodeAlternateElement(pagerId));
                });

            builder.Describe("Pager_CurrentPage")
                .OnDisplaying(displaying =>
                {
                    var pager = displaying.Shape.Pager;
                    string pagerId = pager.PagerId;
                    if (!String.IsNullOrWhiteSpace(pagerId))
                        displaying.Shape.Metadata.Alternates.Add("Pager_CurrentPage__" + EncodeAlternateElement(pagerId));
                });

            builder.Describe("Pager_Links")
                .OnDisplaying(displaying =>
                {
                    var pager = displaying.Shape;
                    string pagerId = pager.PagerId;
                    if (!String.IsNullOrWhiteSpace(pagerId))
                        displaying.Shape.Metadata.Alternates.Add("Pager_Links__" + EncodeAlternateElement(pagerId));
                });
        }

        [Shape]
        public IHtmlContent Pager_Links(dynamic Shape, dynamic Display, dynamic New,
            IHtmlHelper Html,
            int Page,
            int PageSize,
            double TotalItemCount,
            int? Quantity,
            object FirstText,
            object PreviousText,
            object NextText,
            object LastText,
            object GapText,
            string PagerId
            // parameter omitted to workaround an issue where a NullRef is thrown
            // when an anonymous object is bound to an object shape parameter
            /*object RouteValues*/)
        {

            var currentPage = Page;
            if (currentPage < 1)
                currentPage = 1;

            var pageSize = PageSize;

            var numberOfPagesToShow = Quantity ?? 0;
            if (Quantity == null || Quantity < 0)
                numberOfPagesToShow = 7;

            var totalPageCount = pageSize > 0 ? (int)Math.Ceiling(TotalItemCount / pageSize) : 1;

            var firstText = FirstText ?? T["<<"];
            var previousText = PreviousText ?? T["<"];
            var nextText = NextText ?? T[">"];
            var lastText = LastText ?? T[">>"];
            var gapText = GapText ?? T["..."];

            var routeData = new RouteValueDictionary(Html.ViewContext.RouteData.Values);
            var queryString = _httpContextAccessor.HttpContext.Request.Query;
            if (queryString != null)
            {
                foreach (var key in from string key in queryString.Keys where key != null && !routeData.ContainsKey(key) let value = queryString[key] select key)
                {
                    routeData[key] = queryString[key];
                }
            }

            // specific cross-requests route data can be passed to the shape directly (e.g., Orchard.Users)
            var shapeRoute = (object)Shape.RouteData;

            if (shapeRoute != null)
            {
                var shapeRouteData = shapeRoute as RouteValueDictionary;
                if (shapeRouteData == null)
                {
                    var route = shapeRoute as RouteData;
                    if (route != null)
                    {
                        shapeRouteData = new RouteValueDictionary(route.Values);
                    }
                }

                if (shapeRouteData != null)
                {
                    foreach (var rd in shapeRouteData)
                    {
                        routeData[rd.Key] = rd.Value;
                    }
                }
            }
            
            int firstPage = Math.Max(1, Page - (numberOfPagesToShow / 2));
            int lastPage = Math.Min(totalPageCount, Page + (int)(numberOfPagesToShow / 2));

            var pageKey = String.IsNullOrEmpty(PagerId) ? "page" : PagerId;

            Shape.Classes.Add("pager");
            Shape.Metadata.Alternates.Clear();
            Shape.Metadata.Type = "List";

            // first and previous pages
            if (Page > 1)
            {
                if (routeData.ContainsKey(pageKey))
                {
                    routeData.Remove(pageKey); // to keep from having "page=1" in the query string
                }
                // first
                Shape.Add(New.Pager_First(Value: firstText, RouteValues: new RouteValueDictionary(routeData), Pager: Shape));

                // previous
                if (currentPage > 2)
                { // also to keep from having "page=1" in the query string
                    routeData[pageKey] = currentPage - 1;
                }
                Shape.Add(New.Pager_Previous(Value: previousText, RouteValues: new RouteValueDictionary(routeData), Pager: Shape));
            }

            // gap at the beginning of the pager
            if (firstPage > 1 && numberOfPagesToShow > 0)
            {
                Shape.Add(New.Pager_Gap(Value: gapText, Pager: Shape));
            }

            // page numbers
            if (numberOfPagesToShow > 0 && lastPage > 1)
            {
                for (var p = firstPage; p <= lastPage; p++)
                {
                    if (p == currentPage)
                    {
                        Shape.Add(New.Pager_CurrentPage(Value: p, RouteValues: new RouteValueDictionary(routeData), Pager: Shape));
                    }
                    else {
                        if (p == 1)
                            routeData.Remove(pageKey);
                        else
                            routeData[pageKey] = p;
                        Shape.Add(New.Pager_Link(Value: p, RouteValues: new RouteValueDictionary(routeData), Pager: Shape));
                    }
                }
            }

            // gap at the end of the pager
            if (lastPage < totalPageCount && numberOfPagesToShow > 0)
            {
                Shape.Add(New.Pager_Gap(Value: gapText, Pager: Shape));
            }

            // next and last pages
            if (Page < totalPageCount)
            {
                // next
                routeData[pageKey] = Page + 1;
                Shape.Add(New.Pager_Next(Value: nextText, RouteValues: new RouteValueDictionary(routeData), Pager: Shape));
                // last
                routeData[pageKey] = totalPageCount;
                Shape.Add(New.Pager_Last(Value: lastText, RouteValues: new RouteValueDictionary(routeData), Pager: Shape));
            }

            return Display(Shape);
        }

        [Shape]
        public IHtmlContent Pager(dynamic Shape, dynamic Display)
        {
            Shape.Metadata.Alternates.Clear();
            Shape.Metadata.Type = "Pager_Links";
            return Display(Shape);
        }

        [Shape]
        public IHtmlContent Pager_First(dynamic Shape, dynamic Display)
        {
            Shape.Metadata.Alternates.Clear();
            Shape.Metadata.Type = "Pager_Link";
            return Display(Shape);
        }

        [Shape]
        public IHtmlContent Pager_Previous(dynamic Shape, dynamic Display)
        {
            Shape.Metadata.Alternates.Clear();
            Shape.Metadata.Type = "Pager_Link";
            return Display(Shape);
        }

        [Shape]
        public IHtmlContent Pager_CurrentPage(IHtmlHelper Html, dynamic Display, object Value)
        {
            var tagBuilder = new TagBuilder("span");
            tagBuilder.InnerHtml.Append(Display(Value));
            return tagBuilder;
        }

        [Shape]
        public IHtmlContent Pager_Next(dynamic Shape, dynamic Display)
        {
            Shape.Metadata.Alternates.Clear();
            Shape.Metadata.Type = "Pager_Link";
            return Display(Shape);
        }

        [Shape]
        public IHtmlContent Pager_Last(dynamic Shape, dynamic Display)
        {
            Shape.Metadata.Alternates.Clear();
            Shape.Metadata.Type = "Pager_Link";
            return Display(Shape);
        }

        [Shape]
        public IHtmlContent Pager_Link(IHtmlHelper Html, dynamic Shape, dynamic Display, object Value)
        {
            Shape.Metadata.Alternates.Clear();
            Shape.Metadata.Type = "ActionLink";
            return Display(Shape);
        }

        [Shape]
        public IHtmlContent ActionLink(UrlHelper Url, dynamic Shape, object Value)
        {
            var RouteValues = (object)Shape.RouteValues;
            RouteValueDictionary rvd;
            if (RouteValues == null)
            {
                rvd = new RouteValueDictionary();
            }
            else
            {
                rvd = RouteValues as RouteValueDictionary ?? new RouteValueDictionary(RouteValues);
            }

            var action = Url.Action((string)rvd["action"], (string)rvd["controller"], rvd);

            IEnumerable<string> classes = Shape.Classes;
            IDictionary<string, string> attributes = Shape.Attributes;
            attributes["href"] = action;
            string id = Shape.Id;
            var tag = Orchard.DisplayManagement.Shapes.Shape.GetTagBuilder("a", id, classes, attributes);

            tag.InnerHtml.Append(CoerceHtmlString(Value));
            return tag;
        }

        [Shape]
        public IHtmlContent Pager_Gap(IHtmlHelper Html, dynamic Display, object Value)
        {
            var tagBuilder = new TagBuilder("span");
            tagBuilder.InnerHtml.Append(Display(Value));
            return tagBuilder;
        }

        private string EncodeAlternateElement(string alternateElement)
        {
            return alternateElement.Replace("-", "__").Replace(".", "_");
        }

        static IHtmlContent CoerceHtmlString(object value)
        {
            if (value == null)
                return null;

            var result = value as IHtmlContent;
            if (result != null)
                return result;

            return new HtmlString(HtmlEncoder.Default.HtmlEncode(value.ToString()));
        }
    }
}
