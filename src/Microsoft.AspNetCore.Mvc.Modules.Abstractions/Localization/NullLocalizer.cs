using Microsoft.Extensions.Localization;

namespace Orchard.Localization
{
    public static class NullLocalizer
    {
        static NullLocalizer()
        {
            Instance = (format, args) => new LocalizedString("localizer", (args == null || args.Length == 0) ? format : string.Format(format, args));
        }

        public static Localizer Instance { get; }
    }
}