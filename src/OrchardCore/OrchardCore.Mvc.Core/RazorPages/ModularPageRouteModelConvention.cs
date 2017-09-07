using System;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace OrchardCore.Mvc.RazorPages
{
    public class ModularPageRouteModelConvention : IPageRouteModelConvention
    {
        private readonly string _pageName;
        private readonly string _route;

        public ModularPageRouteModelConvention(string pageName, string route)
        {
            _pageName = pageName;
            _route = route;
        }

        public void Apply(PageRouteModel model)
        {
            if (!String.IsNullOrEmpty(_pageName) && model.ViewEnginePath.EndsWith(_pageName))
            {
                if (_pageName[0] == '/' && _pageName.Contains("/Pages/") && !_pageName.StartsWith("/Pages/"))
                {
                    foreach (var selector in model.Selectors)
                    {
                        selector.AttributeRouteModel.SuppressLinkGeneration = true;
                    }

                    model.Selectors.Add(new SelectorModel
                    {
                        AttributeRouteModel = new AttributeRouteModel
                        {
                            Template = _route
                        }
                    });
                }
            }
        }
    }

    public static class PageConventionCollectionExtensions
    {
        public static PageConventionCollection AddModularPageRoute(this PageConventionCollection conventions, string pageName, string route)
        {
            conventions.Add(new ModularPageRouteModelConvention(pageName, route));
            return conventions;
        }
    }
}
