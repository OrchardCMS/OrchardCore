using System;
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

        public override void GetContentItemMetadata(ContentItemMetadataContext context, IdentityPart part)
        {
            context.Metadata.Identity.Add("Identifier", part.Identifier);
        }

        protected void AssignIdentity(IdentityPart part)
        {
            part.Identifier = Guid.NewGuid().ToString("n");
        }
    }
}
