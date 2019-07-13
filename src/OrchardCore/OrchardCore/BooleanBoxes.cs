namespace OrchardCore.Environment.Cache
{
    public class BooleanBoxes
    {
        public static readonly object TrueBox = true;

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