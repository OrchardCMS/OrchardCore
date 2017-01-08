using System;
using System.Security;
using System.Runtime.InteropServices;

namespace Microsoft.AspNetCore.Modules
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