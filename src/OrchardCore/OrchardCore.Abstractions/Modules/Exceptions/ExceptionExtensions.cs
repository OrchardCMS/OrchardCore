using System;
using System.Security;
using System.Runtime.InteropServices;

namespace OrchardCore.Modules
{
    public static class ExceptionExtensions
    {
        public static bool IsFatal(this Exception ex)
        {
            return 
                ex is OutOfMemoryException ||
                ex is SecurityException ||
                ex is SEHException;
        }
    }
}