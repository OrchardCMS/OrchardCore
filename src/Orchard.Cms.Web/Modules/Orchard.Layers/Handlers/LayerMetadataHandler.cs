using System;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Cache;
using Orchard.Layers.Models;

namespace Orchard.Layers.Handlers
{
	public class LayerMetadataHandler : ContentHandlerBase
    {
		public const string LayerChangeToken = "Orchard.Layers:LayerMetadata";

		private readonly ISignal _signal;

		public LayerMetadataHandler(ISignal signal)
		{
			_signal = signal;
		}

		public override void Published(PublishContentContext context)
		{
			SignalLayerChanged(context.ContentItem);
		}

		public override void Removed(RemoveContentContext context)
		{
			SignalLayerChanged(context.ContentItem);
		}

		public override void Unpublished(PublishContentContext context)
		{
			SignalLayerChanged(context.ContentItem);
		}

		private void SignalLayerChanged(ContentItem contentItem)
		{
			var layerMetadata = contentItem.As<LayerMetadata>();

			if (layerMetadata != null)
			{
				_signal.SignalToken(LayerChangeToken);
			}
		}

		public override void GetContentItemAspect(ContentItemAspectContext context)
        {
            context.For<ContentItemMetadata>(metadata =>
            {
				var layerMetadata = context.ContentItem.As<LayerMetadata>();

				if (layerMetadata != null && !String.IsNullOrEmpty(layerMetadata.Title))
				{
					metadata.DisplayText = layerMetadata.Title;
				}
			});
        }
    }
}