namespace OrchardCore.Media.Fields
{
    /// <summary>
    /// An anchor represents two floats with an <see cref="X"/> and <see cref="Y"/> position.
    /// When anchoring is enabled the position defaults to the center of the media.
    /// </summary>
    public class Anchor
    {
        public float X { get; set; } = 0.5f;
        public float Y { get; set; } = 0.5f;
    }
}
