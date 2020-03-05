namespace OrchardCore.ContentManagement.GraphQL.Queries.Predicates
{
    /// <summary>
    /// Represents a strategy for matching strings using the "like" operand.
    /// </summary>
    public abstract class MatchOptions
    {
        /// <summary>
        /// Match the start of the string to the pattern
        /// </summary>
        public static readonly MatchOptions StartsWith = new StartsWithMatchOptions();

        /// <summary>
        /// Match the end of the string to the pattern
        /// </summary>
        public static readonly MatchOptions EndsWith = new EndsWithMatchOptions();

        /// <summary>
        /// Match when the string contains the pattern
        /// </summary>
        public static readonly MatchOptions Contains = new ContainsMatchOptions();

        /// <summary>
        /// Convert the pattern, by appending/prepending "%"
        /// </summary>
        /// <param name="pattern">The string to convert to the appropriate match pattern.</param>
        /// <returns>
        /// A <see cref="string" /> that contains a "%" in the appropriate place
        /// for the Match Strategy.
        /// </returns>
        public abstract string ToMatchString(string pattern);

        /// <summary>
        /// The <see cref="MatchOptions" /> that matches if the string starts with the pattern.
        /// </summary>
        private class StartsWithMatchOptions : MatchOptions
        {
            /// <inheritdoc />
            public override string ToMatchString(string pattern)
            {
                return pattern + '%';
            }
        }

        /// <summary>
        /// The <see cref="MatchOptions" /> that matches if the string ends with the pattern.
        /// </summary>
        private class EndsWithMatchOptions : MatchOptions
        {
            /// <inheritdoc />
            public override string ToMatchString(string pattern)
            {
                return '%' + pattern;
            }
        }

        /// <summary>
        /// The <see cref="MatchOptions" /> that matches if the string contains the pattern.
        /// </summary>
        private class ContainsMatchOptions : MatchOptions
        {
            /// <inheritdoc />
            public override string ToMatchString(string pattern)
            {
                return '%' + pattern + '%';
            }
        }
    }
}
