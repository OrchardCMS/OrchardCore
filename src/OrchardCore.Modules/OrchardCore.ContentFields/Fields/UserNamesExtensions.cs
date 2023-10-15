namespace OrchardCore.ContentFields.Fields
{
    public static class UserNamesExtensions
    {
        private const string PropertyName = "UserNames";

        /// <summary>
        /// User names are a less well known property of a <see cref="UserPickerField"/>
        /// </summary>
        /// <remarks>
        /// This property is stored when the <see cref="UserPickerField"/> is saved, not when the <see cref="OrchardCore.Users.Models.User.UserName"/> value changes.
        /// </remarks>
        public static string[] GetUserNames(this UserPickerField userPickerField) =>
            userPickerField.GetArrayProperty<string>(PropertyName);

        /// <summary>
        /// User names are a less well known property of a <see cref="UserPickerField"/>
        /// </summary>
        /// <remarks>
        /// This property is stored when the <see cref="UserPickerField"/> is saved, not when the <see cref="OrchardCore.Users.Models.User.UserName"/> value changes.
        /// </remarks>
        public static void SetUserNames(this UserPickerField userPickerField, string[] userNames) =>
            userPickerField.SetProperty(PropertyName, userNames);
    }
}
