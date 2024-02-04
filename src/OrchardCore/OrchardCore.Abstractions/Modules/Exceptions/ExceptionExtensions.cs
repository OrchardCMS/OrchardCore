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
                ex is
                    OutOfMemoryException or
                    SecurityException or
                    SEHException;
        }

        public static bool IsFileSharingViolation(this Exception ex) =>
            ex is IOException &&
            ex.HResult is
                ERROR_SHARING_VIOLATION or
                ERROR_LOCK_VIOLATION;
    }
}
