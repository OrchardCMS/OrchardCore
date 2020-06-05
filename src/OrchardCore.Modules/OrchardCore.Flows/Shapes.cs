using Microsoft.AspNetCore.Routing;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Flows.Models;

namespace OrchardCore.Flows
{
    public class Shapes : IShapeTableProvider
    {
        public void Discover(ShapeTableBuilder builder)
        {
            builder.Describe("ContentCard")
            .OnCreated(async context =>
            {
                dynamic cardShape = context.Shape;

                IShape toggleTool = await context.ShapeFactory.New.ToolToggleWidgets();
                toggleTool.Metadata.Name = "ToolToggleWidgets";

                IShape insertTool = await context.ShapeFactory.New.ToolInsertWidget();
                insertTool.Metadata.Name = "ToolInsertWidget";

                cardShape.Footer.Add(toggleTool, "after");
                cardShape.Footer.Add(insertTool, "after");

            })
            .OnDisplaying(context =>
           {
               dynamic cardShape = context.Shape;

               // Set Default values for ToolInsertWidget
               cardShape.Footer.ToolInsertWidget.CanInsert = cardShape.CanInsert;
               cardShape.Footer.ToolInsertWidget.AppendWidget = false;
               cardShape.Footer.ToolInsertWidget.TargetId = cardShape.TargetId;
               cardShape.Footer.ToolInsertWidget.WidgetContentTypes = cardShape.WidgetContentTypes;
               cardShape.Footer.ToolInsertWidget.BuildEditorRouteValues = new RouteValueDictionary(new
               {
                   // Set default to flows module
                   area = cardShape.InsertArea ?? "OrchardCore.Flows",

                   prefixesName = cardShape.PrefixesName,
                   contentTypesName = cardShape.ContentTypesName,
                   targetId = cardShape.TargetId,
                   parentContentType = cardShape.ParentContentType,
                   partName = cardShape.CollectionPartName,

                   //Flow Specific If Any
                   flowmetadata = cardShape.HasFlowMetadata,

                   //Zone Specific In Any
                   zone = cardShape.ZoneValue,
                   zonesName = cardShape.ZonesName
               });

               if (cardShape.CollectionShapeType != nameof(FlowPart))
               {
                   // Remove Insert Widget and Toggle Widgets If it's not FlowPart
                   cardShape.Footer.Remove("ToolInsertWidget");
                   cardShape.Footer.Remove("ToolToggleWidgets");
               }

           });
        }

    }
}
