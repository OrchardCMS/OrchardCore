using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;

namespace OrchardCore.Modules
{
    public static class ExceptionExtensions
    {
        // As defined in https://github.com/dotnet/runtime/blob/main/src/libraries/Common/src/System/HResults.cs

        internal const int ERROR_SHARING_VIOLATION = unchecked((int)0x80070020);
        internal const int ERROR_LOCK_VIOLATION = unchecked((int)0x80070021);

        public static bool IsFatal(this Exception ex)
        {
            return
                ex is OutOfMemoryException ||
                ex is SecurityException ||
                ex is SEHException;
        }

        public static bool IsFileSharingViolation(this Exception ex) =>
            ex is IOException &&
            (ex.HResult == ERROR_SHARING_VIOLATION ||
            ex.HResult == ERROR_LOCK_VIOLATION);
    }
}
