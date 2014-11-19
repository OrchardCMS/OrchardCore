using Microsoft.AspNet.Mvc.Rendering;

namespace OrchardVNext.Localization {
    public class LocalizedString : HtmlString {
        private readonly string _scope;
        private readonly string _textHint;
        private readonly object[] _args;

        public LocalizedString(string languageNeutral) : base(languageNeutral) {
            _textHint = languageNeutral;
        }

        public LocalizedString(string localized, string scope, string textHint, object[] args) : base(localized) {
            _scope = scope;
            _textHint = textHint;
            _args = args;
        }

        public static LocalizedString TextOrDefault(string text, LocalizedString defaultValue) {
            if (string.IsNullOrEmpty(text))
                return defaultValue;
            return new LocalizedString(text);
        }

        public string Scope {
            get { return _scope; }
        }

        public string TextHint {
            get { return _textHint; }
        }

        public object[] Args {
            get { return _args; }
        }

        public string Text {
            get { return ToString(); }
        }

        public override int GetHashCode() {
            var hashCode = 0;
            if (ToString() != null)
                hashCode ^= ToString().GetHashCode();
            return hashCode;
        }

        public override bool Equals(object obj) {
            if (obj == null || obj.GetType() != GetType())
                return false;

            var that = (LocalizedString)obj;
            return string.Equals(ToString(), that.ToString());
        }

    }
}
