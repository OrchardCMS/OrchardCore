using System.Threading.Tasks;
using GraphQL.Types;

namespace OrchardCore.Apis.GraphQL
{
    /// <summary>
    /// Implementations of this interface can participate in building the <see cref="ISchema"/> instance that is used for GraphQL requests.
    /// </summary>
    public interface ISchemaBuilder
    {
        /// <summary>
        /// Updates <paramref name="schema"/>.
        /// </summary>
        /// <param name="schema">The <see cref="ISchema"/> instance to update.</param>
        Task BuildAsync(ISchema schema);

        /// <summary>
        /// Returns an unique identifier that is updated when the data that is used in the <see cref="ISchema"/> instance
        /// has changed, or an empty string if it has no dependencies, null being a valid value not yet updated.
        /// </summary>
        Task<string> GetIdentifierAsync();
    }
}
