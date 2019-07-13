namespace OrchardCore
{
    /// <summary>
    /// Provides reusable boxed booleans.
    /// </summary>
    public static class BooleanBoxes
    {
        /// <summary>
        /// Static boxed instance for true.
        /// </summary>
        public static readonly object TrueBox = true;

        /// <summary>
        /// Static boxed instance for false.
        /// </summary>
        public static readonly object FalseBox = false;

        public static object Box(bool value)
        {
            return value ? TrueBox : FalseBox;
        }

        public static object Box(bool? boolean)
        {
            return boolean.HasValue
                ? Box(boolean.Value)
                : null;
        }
    }
}