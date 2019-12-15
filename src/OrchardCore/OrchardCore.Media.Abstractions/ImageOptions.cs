namespace OrchardCore.Media
{
    public class ImageOptions
    {
        /// <summary>
        /// The image need to be transformed or not.
        /// </summary>
        public bool NeedTransformImage { get; set; }

        /// <summary>
        /// Any dynamic stuff used for transforming the image.
        /// </summary>
        public dynamic Options { get; set; }
    }
}
