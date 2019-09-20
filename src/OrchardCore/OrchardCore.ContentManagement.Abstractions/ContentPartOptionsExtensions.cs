using System;
using System.Linq;

namespace OrchardCore.ContentManagement
{
    public static class ContentPartOptionsExtensions
    {
        public static ContentPartOptions AddPart<TContentPart>(this ContentPartOptions contentPartOptions)
            where TContentPart : ContentPart
        {
            var option = new ContentPartOption<TContentPart>();

            contentPartOptions.PartOptions = contentPartOptions.PartOptions.Union(new[] { option });

            return contentPartOptions;
        }

        public static ContentPartOptions AddPart<TContentPart>(this ContentPartOptions contentPartOptions, Action<ContentPartOption> action)
            where TContentPart : ContentPart
        {
            var option = new ContentPartOption<TContentPart>();

            action(option);

            contentPartOptions.PartOptions = contentPartOptions.PartOptions.Union(new[] { option });

            return contentPartOptions;
        }
    }
}
