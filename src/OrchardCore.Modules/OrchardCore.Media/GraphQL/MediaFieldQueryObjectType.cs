using System.Collections.Generic;
using System.Linq;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Media;
using OrchardCore.Media.Fields;

namespace OrchardCore.Markdown.GraphQL
{
    public class MediaFieldQueryObjectType : ObjectGraphType<MediaField>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MediaFieldQueryObjectType(IHttpContextAccessor httpContextAccessor)
        {
            Name = nameof(MediaField);

            Field("paths", x => x.Paths, nullable: true)
                .Description("the media paths")
                .Type(new ListGraphType<StringGraphType>())
                ;

            Field("urls", x => ToUrl(x.Paths), nullable: true)
                .Description("the absolute urls of the media items")
                .Type(new ListGraphType<StringGraphType>())
                ;
            _httpContextAccessor = httpContextAccessor;
        }

        private List<string> ToUrl(string[] path)
        {
            var mediaFileStore = _httpContextAccessor.HttpContext.RequestServices.GetService<IMediaFileStore>();
            return path.Select(x => mediaFileStore.MapPathToPublicUrl(x)).ToList();
        }
    }
}
