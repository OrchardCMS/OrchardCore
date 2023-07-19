using System;
using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Mvc.Core.Utilities
{
    public static class ControllerTypeExtensions
    {
        public static string ControllerName(this Type controllerType)
        {
            if (!typeof(Controller).IsAssignableFrom(controllerType))
            {
                throw new ArgumentException("The specified type must inherit from " + nameof(Controller), nameof(controllerType));
            }

            return controllerType.Name.EndsWith(nameof(Controller), StringComparison.OrdinalIgnoreCase)
                ? controllerType.Name[..^nameof(Controller).Length]
                : controllerType.Name;
        }
    }
}
