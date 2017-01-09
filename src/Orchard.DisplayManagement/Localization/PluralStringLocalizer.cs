namespace Microsoft.Extensions.Localization
{
    public class PluralStringLocalizer<T> : IPluralStringLocalizer<T>
    {
        private readonly IStringLocalizer<T> _stringLocalizer;

        public PluralStringLocalizer(IStringLocalizer<T> stringLocalizer)
        {
            _stringLocalizer = stringLocalizer;
        }

        public LocalizedString this[string name, string pluralName, int count, params object[] arguments]
        {
            get
            {
                if (count > 1)
                {
                    return _stringLocalizer[pluralName, arguments];
                }
                else
                {
                    return _stringLocalizer[name, arguments];
                }
            }
        }
    }
}
