using System.Runtime.InteropServices;
using System.Security;
using Microsoft.Extensions.Logging;

namespace OrchardCore.Modules;

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

    public static void LogException(this Exception ex, ILogger logger, Type sourceType, string method)
    {
        logger.LogError(ex, "{Exception} thrown from {Type} by {Method}", ex.GetType().Name, sourceType.Name, method);
    }
}
