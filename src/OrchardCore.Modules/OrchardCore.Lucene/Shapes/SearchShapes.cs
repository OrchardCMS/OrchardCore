using System;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Search.Abstractions.ViewModels;

namespace OrchardCore.Lucene
{
    public class SearchShapes : IShapeTableProvider
    {
        public void Discover(ShapeTableBuilder builder)
        {
            builder.Describe("SearchForm")
                .OnDisplaying(context =>
                {
                    dynamic searchForm = context.Shape;
                });

            builder.Describe("SearchResults")
                .OnDisplaying(context =>
                {
                    dynamic searchResults = context.Shape;
                });
        }
    }
}
