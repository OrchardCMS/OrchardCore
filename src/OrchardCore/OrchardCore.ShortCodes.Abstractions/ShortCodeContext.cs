namespace OrchardCore.ShortCodes
{
    public class ShortCodeContext
    {
        public ShortCodeContext(string shortCodeName = "") // TODO: Add shortcode name for the current context
        {
            ShortCodeName = shortCodeName;
        }

        public string ShortCodeName { get; }
    }
}
