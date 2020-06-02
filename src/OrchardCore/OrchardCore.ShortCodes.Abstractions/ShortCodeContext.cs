namespace OrchardCore.ShortCodes
{
    public class ShortCodeContext
    {
        public ShortCodeContext(string shortCodeName)
        {
            ShortCodeName = shortCodeName;
        }

        public string ShortCodeName { get; }
    }
}
