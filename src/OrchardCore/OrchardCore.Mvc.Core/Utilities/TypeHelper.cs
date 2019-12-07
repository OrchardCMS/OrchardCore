using System;
using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Mvc.Core.Utilities
{
    public static class TypeHelper
    {
        public static string GetControllerName<T>() where T : Controller
        {
            var controllerTypeName = typeof(T).Name;

            return controllerTypeName.EndsWith("Controller", StringComparison.OrdinalIgnoreCase)
                ? controllerTypeName.Substring(0, controllerTypeName.Length - "Controller".Length)
                : controllerTypeName;
        }
    }
}
