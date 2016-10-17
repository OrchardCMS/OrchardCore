namespace Orchard.Tokens.Services
{
    public interface ITokenizer
    {
        /// <summary>
        /// Evaluates the template with the specified data object as the context.
        /// </summary>
        string Tokenize(string template, object context);

        /// <summary>
        /// Creates a dynamic object that can be used as a context object.
        /// </summary>
        /// <returns></returns>
        dynamic CreateViewModel();
    }
}
