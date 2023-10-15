namespace OrchardCore.Media.Fields
{
    public static class MediaFieldAttachedFileNameExtensions
    {
        private const string PropertyName = "AttachedFileNames";

        /// <summary>
        /// Gets the names of <see cref="MediaField"/> attached files.
        /// </summary>
        public static string[] GetAttachedFileNames(this MediaField mediaField) =>
            mediaField.GetArrayProperty<string>(PropertyName);

        /// <summary>
        /// Sets the names of <see cref="MediaField"/> attached files.
        /// </summary>
        public static void SetAttachedFileNames(this MediaField mediaField, string[] filenames) =>
            mediaField.SetProperty(PropertyName, filenames);

    }
}
