using System;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Identity.Models;

namespace Orchard.Identity.Handlers
{
    public class IdentityPartHandler : ContentPartHandler<IdentityPart>
    {
        public override void Initializing(InitializingContentContext context, IdentityPart part)
        {
            AssignIdentity(part);
        }

        public override void GetContentItemAspect(ContentItemAspectContext context, IdentityPart part)
        {
            context.For<ContentItemMetadata>(contentItemMetadata =>
            {
                contentItemMetadata.Identity.Add("Identifier", part.Identifier);
            });
        }

        protected void AssignIdentity(IdentityPart part)
        {
            part.Identifier = Guid.NewGuid().ToString("n");
        }
    }
}
