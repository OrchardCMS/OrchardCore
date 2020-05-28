using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.Forms
{
    public class Shapes : IShapeTableProvider
    {
        public void Discover(ShapeTableBuilder builder)
        {
            builder.Describe("ContentCard")
            .OnCreated(async context =>
            {

                dynamic cardShape = context.Shape;

                IShape settingTool = await context.ShapeFactory.New.ToolFormWidgetSettings();
                settingTool.Metadata.Name = "ToolFormWidgetSettings";

                settingTool.Properties["Content"] = await context.ShapeFactory.CreateAsync("ContentZone");
                settingTool.Properties["Actions"] = await context.ShapeFactory.CreateAsync("ContentZone");

                cardShape.Footer.Add(settingTool, "before");


                IShape formWorkflowTool = await context.ShapeFactory.New.ToolFormWorkflow();
                formWorkflowTool.Metadata.Name = "ToolFormWorkflow";
                cardShape.Footer.Add(formWorkflowTool, "before");

            })
            .OnDisplaying(context =>
            {
                dynamic cardShape = context.Shape;

                bool hasSettingsTool = false;

                if (cardShape.ContentItem.ContentType == "Form")
                {
                    var flowPart = cardShape.ContentEditor.Parts.FlowPart;
                    var parts = (cardShape.ContentEditor.Parts as Shape);

                    // Currently Shape doesn't have Remove shape by shape object.
                    // So Remove it from Items property ( here Items is List<IPositioned> )
                    var items = parts.Items as System.Collections.IList;
                    items.Remove(flowPart);

                    // Move FlowPart to content
                    cardShape.Content.Add(flowPart);

                    // Move rest of Form editor to Modal
                    cardShape.Footer.ToolFormWidgetSettings.Content.Add(cardShape.ContentEditor);

                    cardShape.Footer.ToolFormWorkflow.ModalId = $"ToolFormWorkflow_{cardShape.ContentItem.ContentItemId}";
                    cardShape.Footer.ToolFormWorkflow.ContentItemDisplayText = cardShape.ContentItem.DisplayText;
                    cardShape.Footer.ToolFormWorkflow.ContentType = cardShape.ContentItem.ContentType;

                    hasSettingsTool = true;
                }
                else
                {
                    // Remove Workflow Tool
                    cardShape.Footer.Remove("ToolFormWorkflow");

                    if (cardShape.ContentItem.ContentType == "ValidationSummary")
                    {

                        cardShape.IsCollapsible = false;

                        // Remove Toggle All and Settings for Validation Summary
                        cardShape.Footer.Remove("ToolToggleWidgets");

                    }

                    else if (cardShape.ContentItem.ContentType == "Label" ||
                                cardShape.ContentItem.ContentType == "Button" ||
                                cardShape.ContentItem.ContentType == "TextArea" ||
                                cardShape.ContentItem.ContentType == "Input" ||
                                cardShape.ContentItem.ContentType == "Validation"
                     )
                    {
                        // Remove Collapse buttons
                        cardShape.IsCollapsible = false;

                        // Remove Toggle All for all non-collapsible widgets
                        cardShape.Footer.Remove("ToolToggleWidgets");

                        //Move Content Inline to Card Header, if any
                        foreach (var item in cardShape.ContentEditor.Inline)
                        {
                            cardShape.Header.Add(item);
                        }

                        cardShape.ContentEditor.Inline = null;

                        // Move rest of editor to Modal
                        cardShape.Footer.ToolFormWidgetSettings.Content.Add(cardShape.ContentEditor);
                        hasSettingsTool = true;

                        // Remove Content Section not to render Body of ContentCard
                        cardShape.Content = null;

                    }
                }

                if (hasSettingsTool)
                {
                    // Set Modal ID for ToolWidgetSettings
                    cardShape.Footer.ToolFormWidgetSettings.ModalId = $"ToolFormWidgetSettings_{cardShape.ContentItem.ContentItemId}";
                    cardShape.Footer.ToolFormWidgetSettings.ContentItemDisplayText = cardShape.ContentItem.DisplayText;
                    cardShape.Footer.ToolFormWidgetSettings.ContentType = cardShape.ContentItem.ContentType;
                }
                else
                {
                    cardShape.Footer.Remove("ToolFormWidgetSettings");
                }
            });
        }

    }
}
