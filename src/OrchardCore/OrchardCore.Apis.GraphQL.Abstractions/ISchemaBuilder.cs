using System.Threading.Tasks;
using GraphQL.Types;
using Microsoft.Extensions.Primitives;

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
        /// <returns>A <see cref="IChangeToken"/> instance that is invalidated when the data that is used in the <see cref="ISchema"/>
        /// instance has changed, or <c>null</c> if it has no dependencies.</returns>
        Task<IChangeToken> BuildAsync(ISchema schema);
    }
}
