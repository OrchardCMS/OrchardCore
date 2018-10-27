using System.Collections.Generic;
using System.Linq;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL;
using OrchardCore.Media.Fields;

namespace OrchardCore.Media.GraphQL
{
    public class MediaFieldQueryObjectType : ObjectGraphType<MediaField>
    {
        public MediaFieldQueryObjectType()
        {
            Name = nameof(MediaField);

            Field("paths", x => x.Paths, nullable: true)
                .Description("the media paths")
                .Type(new StringGraphType())
                ;

            Field<ListGraphType<StringGraphType>, IEnumerable<string>>()
                .Name("urls")
                .Description("the absolute urls of the media items")
                .Resolve(x =>
                {
                    var paths = x.Source.Paths;
                    var context = (GraphQLContext)x.UserContext;
                    var mediaFileStore = context.ServiceProvider.GetService<IMediaFileStore>();
                    return paths.Select(p => mediaFileStore.MapPathToPublicUrl(p));
                });
        }
    }
}
