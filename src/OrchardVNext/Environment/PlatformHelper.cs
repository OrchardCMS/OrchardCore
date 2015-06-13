using System;

namespace OrchardVNext.Environment {
    internal static class PlatformHelper {
        private static Lazy<bool> _isMono = new Lazy<bool>(() => Type.GetType("Mono.Runtime") != null);

        public static bool IsMono {
            get {
                return _isMono.Value;
            }
        }

        public static bool IsWindows {
            get {
                // For now assume Windows = not Mono
                return !IsMono;
            }
        }
    }
}