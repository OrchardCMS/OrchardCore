namespace OrchardCore.ShortCodes
{
    internal class ShortCodeSpan
    {
        public static readonly ShortCodeSpan Default = new ShortCodeSpan(-1, -1);

        public ShortCodeSpan(int start, int end)
        {
            Start = start;
            End = end;
        }

        public int Start { get; }

        public int End { get; }
    }
}
