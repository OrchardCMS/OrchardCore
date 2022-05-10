using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Html;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.Navigation
{
    public class PagerShapesTableProvider : IShapeTableProvider
    {
        public void Discover(ShapeTableBuilder builder)
        {
            builder.Describe("Pager")
                .OnCreated(created =>
                {
                    // Initializes the common properties of a Pager shape
                    // such that views can safely add values to them.
                    created.Shape.Properties["ItemClasses"] = new List<string>();
                    created.Shape.Properties["ItemAttributes"] = new Dictionary<string, string>();
                })
                .OnDisplaying(displaying =>
                {
                    if (displaying.Shape.TryGetProperty("PagerId", out string pagerId) && !String.IsNullOrEmpty(pagerId))
                    {
                        displaying.Shape.Metadata.Alternates.Add("Pager__" + EncodeAlternateElement(pagerId));
                    };
                });

            builder.Describe("PagerSlim")
                .OnCreated(created =>
                {
                    // Initializes the common properties of a Pager shape
                    // such that views can safely add values to them.
                    created.Shape.Properties["ItemClasses"] = new List<string>();
                    created.Shape.Properties["ItemAttributes"] = new Dictionary<string, string>();
                })
                .OnDisplaying(displaying =>
                {
                    if (displaying.Shape.TryGetProperty("PagerId", out string pagerId) && !String.IsNullOrEmpty(pagerId))
                    {
                        displaying.Shape.Metadata.Alternates.Add("Pager__" + EncodeAlternateElement(pagerId));
                    };
                });

            builder.Describe("Pager_Gap")
                .OnDisplaying(displaying =>
                {
                    var shape = displaying.Shape;

                    if (shape.TryGetProperty("Pager", out IShape pager) && pager.TryGetProperty("PagerId", out string pagerId) && !String.IsNullOrEmpty(pagerId))
                    {
                        displaying.Shape.Metadata.Alternates.Add("Pager_Gap__" + EncodeAlternateElement(pagerId));
                    }
                });

            builder.Describe("Pager_First")
                .OnDisplaying(displaying =>
                {
                    var shape = displaying.Shape;

                    if (shape.TryGetProperty("Pager", out IShape pager) && pager.TryGetProperty("PagerId", out string pagerId) && !String.IsNullOrEmpty(pagerId))
                    {
                        displaying.Shape.Metadata.Alternates.Add("Pager_First__" + EncodeAlternateElement(pagerId));
                    }
                });

            builder.Describe("Pager_Previous")
                .OnDisplaying(displaying =>
                {
                    var shape = displaying.Shape;

                    if (shape.TryGetProperty("Pager", out IShape pager) && pager.TryGetProperty("PagerId", out string pagerId) && !String.IsNullOrEmpty(pagerId))
                    {
                        displaying.Shape.Metadata.Alternates.Add("Pager_Previous__" + EncodeAlternateElement(pagerId));
                    }
                });

            builder.Describe("Pager_Next")
                .OnDisplaying(displaying =>
                {
                    var shape = displaying.Shape;

                    if (shape.TryGetProperty("Pager", out IShape pager) && pager.TryGetProperty("PagerId", out string pagerId) && !String.IsNullOrEmpty(pagerId))
                    {
                        displaying.Shape.Metadata.Alternates.Add("Pager_Next__" + EncodeAlternateElement(pagerId));
                    }
                });

            builder.Describe("Pager_Last")
                .OnDisplaying(displaying =>
                {
                    var shape = displaying.Shape;

                    if (shape.TryGetProperty("Pager", out IShape pager) && pager.TryGetProperty("PagerId", out string pagerId) && !String.IsNullOrEmpty(pagerId))
                    {
                        displaying.Shape.Metadata.Alternates.Add("Pager_Last__" + EncodeAlternateElement(pagerId));
                    }
                });

            builder.Describe("Pager_CurrentPage")
                .OnDisplaying(displaying =>
                {
                    var shape = displaying.Shape;

                    if (shape.TryGetProperty("Pager", out IShape pager) && pager.TryGetProperty("PagerId", out string pagerId) && !String.IsNullOrEmpty(pagerId))
                    {
                        displaying.Shape.Metadata.Alternates.Add("Pager_CurrentPage__" + EncodeAlternateElement(pagerId));
                    }
                });

            builder.Describe("Pager_Links")
                .OnDisplaying(displaying =>
                {
                    if (displaying.Shape.TryGetProperty("PagerId", out string pagerId))
                    {
                        displaying.Shape.Metadata.Alternates.Add("Pager_Links__" + EncodeAlternateElement(pagerId));
                    }
                });
        }

        private static string EncodeAlternateElement(string alternateElement)
        {
            return alternateElement.Replace("-", "__").Replace('.', '_');
        }
    }

    public class PagerShapes : IShapeAttributeProvider
    {
        private readonly IStringLocalizer S;

        public PagerShapes(IStringLocalizer<PagerShapes> localizer)
        {
            S = localizer;
        }

        [Shape]
        public async Task<IHtmlContent> Pager_Links(Shape shape, DisplayContext displayContext, IShapeFactory shapeFactory, IHtmlHelper Html,
            string PagerId,
            int Page,
            int PageSize,
            double TotalItemCount,
            int? Quantity,
            object FirstText,
            object PreviousText,
            object NextText,
            object LastText,
            object GapText,
            bool ShowNext)
        {
            var noFollow = shape.Attributes.ContainsKey("rel") && shape.Attributes["rel"] == "no-follow";
            var currentPage = Page;
            if (currentPage < 1)
                currentPage = 1;

            var pageSize = PageSize;

            var numberOfPagesToShow = Quantity ?? 0;
            if (Quantity == null || Quantity < 0)
                numberOfPagesToShow = 7;

            var totalPageCount = pageSize > 0 ? (int)Math.Ceiling(TotalItemCount / pageSize) : 1;

            // return shape early if pager is not needed.
            if (totalPageCount < 2)
            {
                shape.Metadata.Type = "List";
                return await displayContext.DisplayHelper.ShapeExecuteAsync(shape);
            }

            var firstText = FirstText ?? S["<<"];
            var previousText = PreviousText ?? S["<"];
            var nextText = NextText ?? S[">"];
            var lastText = LastText ?? S[">>"];
            var gapText = GapText ?? S["..."];

            var httpContextAccessor = displayContext.ServiceProvider.GetService<IHttpContextAccessor>();
            var httpContext = httpContextAccessor.HttpContext;

            var routeData = new RouteValueDictionary(Html.ViewContext.RouteData.Values);

            if (httpContext != null)
            {
                var queryString = httpContext.Request.Query;
                if (queryString != null)
                {
                    foreach (var key in from string key in queryString.Keys where key != null && !routeData.ContainsKey(key) let value = queryString[key] select key)
                    {
                        routeData[key] = queryString[key];
                    }
                }
            }

            // specific cross-requests route data can be passed to the shape directly (e.g., OrchardCore.Users)
            var shapeRouteData = shape.GetProperty<RouteData>("RouteData");
            if (shapeRouteData != null)
            {
                foreach (var rd in shapeRouteData.Values)
                {
                    routeData[rd.Key] = rd.Value;
                }
            }

            var firstPage = Math.Max(1, Page - (numberOfPagesToShow / 2));
            var lastPage = Math.Min(totalPageCount, Page + (numberOfPagesToShow / 2));

            var pageKey = String.IsNullOrEmpty(PagerId) ? "pagenum" : PagerId;

            shape.Classes.Add("pager");
            shape.Metadata.Alternates.Clear();
            shape.Metadata.Type = "List";

            // first and previous pages
            if ((Page > 1) && (routeData.ContainsKey(pageKey)))
            {
                routeData.Remove(pageKey); // to keep from having "pagenum=1" in the query string
            }

            // first
            var firstItem = await shapeFactory.CreateAsync("Pager_First", Arguments.From(new
            {
                Value = firstText,
                RouteValues = new RouteValueDictionary(routeData),
                Pager = shape,
                Disabled = Page < 2
            }));

            if (noFollow)
            {
                firstItem.Attributes["rel"] = "no-follow";
            }

            await shape.AddAsync(firstItem);

            // previous
            if ((Page > 1) && (currentPage > 2))
            { // also to keep from having "pagenum=1" in the query string
                routeData[pageKey] = currentPage - 1;
            }

            var previousItem = await shapeFactory.CreateAsync("Pager_Previous", Arguments.From(new
            {
                Value = previousText,
                RouteValues = new RouteValueDictionary(routeData),
                Pager = shape,
                Disabled = Page < 2
            }));

            if (noFollow)
            {
                previousItem.Attributes["rel"] = "no-follow";
            }

            await shape.AddAsync(previousItem);

            // gap at the beginning of the pager
            if (firstPage > 1 && numberOfPagesToShow > 0)
            {
                await shape.AddAsync(await shapeFactory.CreateAsync("Pager_Gap", Arguments.From(new
                {
                    Value = gapText,
                    Pager = shape
                })));
            }

            // page numbers
            if (numberOfPagesToShow > 0 && lastPage > 1)
            {
                for (var p = firstPage; p <= lastPage; p++)
                {
                    if (p == 1)
                    {
                        // to keep from having "pagenum=1" in the query string
                        routeData.Remove(pageKey);
                    }
                    else
                    {
                        routeData[pageKey] = p;
                    }

                    if (p == currentPage)
                    {
                        var currentPageItem = await shapeFactory.CreateAsync("Pager_CurrentPage", Arguments.From(new
                        {
                            Value = p,
                            RouteValues = new RouteValueDictionary(routeData),
                            Pager = shape
                        }));

                        if (noFollow)
                        {
                            currentPageItem.Attributes["rel"] = "no-follow";
                        }

                        await shape.AddAsync(currentPageItem);
                    }
                    else
                    {
                        var pagerItem = await shapeFactory.CreateAsync("Pager_Link", Arguments.From(new
                        {
                            Value = p,
                            RouteValues = new RouteValueDictionary(routeData),
                            Pager = shape
                        }));

                        if (p > currentPage)
                        {
                            pagerItem.Attributes["rel"] = noFollow ? "no-follow" : "next";
                        }
                        else if (p < currentPage)
                        {
                            pagerItem.Attributes["rel"] = noFollow ? "no-follow" : "prev";
                        }

                        await shape.AddAsync(pagerItem);
                    }
                }
            }

            // gap at the end of the pager
            if (lastPage < totalPageCount && numberOfPagesToShow > 0)
            {
                await shape.AddAsync(await shapeFactory.CreateAsync("Pager_Gap", Arguments.From(new
                {
                    Value = gapText,
                    Pager = shape
                })));
            }

            // Next
            routeData[pageKey] = Page + 1;
            var pagerNextItem = await shapeFactory.CreateAsync("Pager_Next", Arguments.From(new
            {
                Value = nextText,
                RouteValues = new RouteValueDictionary(routeData),
                Pager = shape,
                Disabled = Page >= totalPageCount && !ShowNext
            }));

            if (noFollow)
            {
                pagerNextItem.Attributes["rel"] = "no-follow";
            }

            await shape.AddAsync(pagerNextItem);

            // Last
            routeData[pageKey] = totalPageCount;
            var pagerLastItem = await shapeFactory.CreateAsync("Pager_Last", Arguments.From(new
            {
                Value = lastText,
                RouteValues = new RouteValueDictionary(routeData),
                Pager = shape,
                Disabled = Page >= totalPageCount
            }));

            if (noFollow)
            {
                pagerLastItem.Attributes["rel"] = "no-follow";
            }

            await shape.AddAsync(pagerLastItem);

            return await displayContext.DisplayHelper.ShapeExecuteAsync(shape);
        }

        [Shape]
        public Task<IHtmlContent> Pager(Shape shape, DisplayContext displayContext)
        {
            shape.Metadata.Alternates.Clear();
            shape.Metadata.Type = "Pager_Links";
            return displayContext.DisplayHelper.ShapeExecuteAsync(shape);
        }

        [Shape]
        public async Task<IHtmlContent> PagerSlim(Shape shape, DisplayContext displayContext, IShapeFactory shapeFactory, IHtmlHelper Html,
            object PreviousText,
            object NextText,
            string PreviousClass,
            string NextClass,
            Dictionary<string, string> UrlParams)
        {
            var noFollow = shape.Attributes.ContainsKey("rel") && shape.Attributes["rel"] == "no-follow";
            var previousText = PreviousText ?? S["<"];
            var nextText = NextText ?? S[">"];

            shape.Classes.Add("pager");
            shape.Metadata.Alternates.Clear();
            shape.Metadata.Type = "List";

            var routeData = new RouteValueDictionary(Html.ViewContext.RouteData.Values);

            // Allows to pass custom url params to PagerSlim
            if (UrlParams != null)
            {
                foreach (var item in UrlParams)
                {
                    routeData.Add(item.Key, item.Value);
                }
            }

            if (shape.TryGetProperty("Before", out string before))
            {
                var beforeRouteData = new RouteValueDictionary(routeData)
                {
                    ["before"] = before
                };

                var previousItem = await shapeFactory.CreateAsync("Pager_Previous", Arguments.From(new
                {
                    Value = previousText,
                    RouteValues = beforeRouteData,
                    Pager = shape
                }));

                if (noFollow)
                {
                    previousItem.Attributes["rel"] = "no-follow";
                }

                await shape.AddAsync(previousItem);
                shape.Properties["FirstClass"] = PreviousClass;
            }

            if (shape.TryGetProperty("After", out string after))
            {
                var afterRouteData = new RouteValueDictionary(routeData)
                {
                    ["after"] = after
                };

                var nextItem = await shapeFactory.CreateAsync("Pager_Next", Arguments.From(new
                {
                    Value = nextText,
                    RouteValues = afterRouteData,
                    Pager = shape
                }));

                if (noFollow)
                {
                    nextItem.Attributes["rel"] = "no-follow";
                }

                await shape.AddAsync(nextItem);
                shape.Properties["LastClass"] = NextClass;
            }

            return await displayContext.DisplayHelper.ShapeExecuteAsync(shape);
        }

        [Shape]
        public Task<IHtmlContent> Pager_First(Shape shape, DisplayContext displayContext)
        {
            shape.Metadata.Alternates.Clear();
            shape.Metadata.Type = "Pager_Link";
            return displayContext.DisplayHelper.ShapeExecuteAsync(shape);
        }

        [Shape]
        public Task<IHtmlContent> Pager_Previous(Shape shape, DisplayContext displayContext)
        {
            shape.Metadata.Alternates.Clear();
            shape.Metadata.Type = "Pager_Link";

            if (!shape.Attributes.ContainsKey("rel"))
            {
                shape.Attributes["rel"] = "prev";
            }

            return displayContext.DisplayHelper.ShapeExecuteAsync(shape);
        }

        [Shape]
        public Task<IHtmlContent> Pager_CurrentPage(Shape shape, DisplayContext displayContext)
        {
            shape.Metadata.Alternates.Clear();
            shape.Metadata.Type = "Pager_Link";
            var parentTag = shape.GetProperty<TagBuilder>("Tag");
            parentTag.AddCssClass("active");

            return displayContext.DisplayHelper.ShapeExecuteAsync(shape);
        }

        [Shape]
        public Task<IHtmlContent> Pager_Next(Shape shape, DisplayContext displayContext)
        {
            shape.Metadata.Alternates.Clear();
            shape.Metadata.Type = "Pager_Link";

            if (!shape.Attributes.ContainsKey("rel"))
            {
                shape.Attributes["rel"] = "next";
            }

            return displayContext.DisplayHelper.ShapeExecuteAsync(shape);
        }

        [Shape]
        public Task<IHtmlContent> Pager_Last(Shape shape, DisplayContext displayContext)
        {
            shape.Metadata.Alternates.Clear();
            shape.Metadata.Type = "Pager_Link";
            return displayContext.DisplayHelper.ShapeExecuteAsync(shape);
        }

        [Shape]
        public Task<IHtmlContent> Pager_Link(Shape shape, DisplayContext displayContext)
        {
            shape.Metadata.Alternates.Clear();
            shape.Metadata.Type = "ActionLink";
            return displayContext.DisplayHelper.ShapeExecuteAsync(shape);
        }

        [Shape]
        public IHtmlContent ActionLink(Shape shape, IUrlHelper Url, object Value, bool Disabled = false)
        {
            if (Disabled)
            {
                if (shape.TryGetProperty("Tag", out TagBuilder tagBuilder))
                {
                    tagBuilder.AddCssClass("disabled");
                }
            }

            var routeValues = shape.GetProperty<RouteValueDictionary>("RouteValues") ?? new RouteValueDictionary();
            if (!Disabled)
            {
                shape.Attributes["href"] = Url.Action((string)routeValues["action"], (string)routeValues["controller"], routeValues);
            }
            else
            {
                shape.Attributes.Remove("href");
            }

            var tag = shape.GetTagBuilder("a");

            tag.InnerHtml.AppendHtml(CoerceHtmlString(Value));
            return tag;
        }

        [Shape]
        public Task<IHtmlContent> Pager_Gap(IShape shape, DisplayContext displayContext)
        {
            shape.Metadata.Alternates.Clear();
            shape.Metadata.Type = "Pager_Link";
            var parentTag = shape.GetProperty<TagBuilder>("Tag");
            parentTag.AddCssClass("disabled");
            return displayContext.DisplayHelper.ShapeExecuteAsync(shape);
        }

        private static IHtmlContent CoerceHtmlString(object value)
        {
            if (value == null)
            {
                return HtmlString.Empty;
            }

            if (value is IHtmlContent result)
            {
                return result;
            }

            return new HtmlContentString(value.ToString());
        }
    }
}
