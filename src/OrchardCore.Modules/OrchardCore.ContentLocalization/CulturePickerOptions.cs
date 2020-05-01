namespace OrchardCore.ContentLocalization
{
    public class CulturePickerOptions
    {
        public static readonly int DefaultCookieLifeTime = 14;

        public int CookieLifeTime { get; set; } = DefaultCookieLifeTime;
    }
}
