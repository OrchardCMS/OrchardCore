namespace OrchardCore.Liquid
{
    /// <summary>
    /// This is a placeholder class that allows modules to extend the `Content` property in the current Liquid scope
    /// </summary>
    public class LiquidContentAccessor
    {
        private readonly string _content;

        public LiquidContentAccessor(string content) => _content = content;

        public override string ToString() => _content;
    }
}
