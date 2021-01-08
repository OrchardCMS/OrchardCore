namespace OrchardCore.Liquid
{
    /// <summary>
    /// This is a placeholder class that allows modules to extend the `Content` property in the current Liquid scope
    /// </summary>
    public class LiquidContentAccessor
    {
        public LiquidContentAccessor(string content) => Content = content;

        public string Content { get; set; }

        public override string ToString() => Content;
    }
}
