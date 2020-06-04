using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;

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

                IShape settingTool = await context.ShapeFactory.New.ToolFormSettings();
                settingTool.Metadata.Name = "ToolFormSettings";

                settingTool.Properties["Content"] = await context.ShapeFactory.CreateAsync("ContentZone");
                settingTool.Properties["Actions"] = await context.ShapeFactory.CreateAsync("ContentZone");

                cardShape.Footer.Add(settingTool, "before");


                IShape formWorkflowTool = await context.ShapeFactory.New.ToolFormWorkflow();
                formWorkflowTool.Metadata.Name = "ToolFormWorkflow";

                formWorkflowTool.Properties["Content"] = await context.ShapeFactory.CreateAsync("ContentZone");
                formWorkflowTool.Properties["Actions"] = await context.ShapeFactory.CreateAsync("ContentZone");


                cardShape.Footer.Add(formWorkflowTool, "before");

            })
            .OnDisplaying(context =>
            {
                dynamic cardShape = context.Shape;

                bool hasSettingsTool = false;

                if (cardShape.ContentItem.ContentType == "Form")
                {
                    //Move WorkflowSettings to ToolFormWorkflow Content, if any
                    foreach (var item in cardShape.ContentEditor.WorkflowSettings)
                    {
                        cardShape.Footer.ToolFormWorkflow.Content.Add(item);
                    }
                    cardShape.ContentEditor.WorkflowSettings = null;

                    //Configure ToolFormWorkflow Shape
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

                        hasSettingsTool = true;

                        // Remove Content Section not to render Body of ContentCard
                        cardShape.Content = null;

                    }
                }

                if (hasSettingsTool)
                {
                    // Move FormSettings to ToolFormSettings Footer, if any
                    foreach (var item in cardShape.ContentEditor.FormSettings)
                    {
                        cardShape.Footer.ToolFormSettings.Content.Add(item);
                    }
                    cardShape.ContentEditor.FormSettings = null;


                    // Set Modal ID for ToolWidgetSettings
                    cardShape.Footer.ToolFormSettings.ModalId = $"ToolFormSettings_{cardShape.ContentItem.ContentItemId}";
                    cardShape.Footer.ToolFormSettings.ContentItemDisplayText = cardShape.ContentItem.DisplayText;
                    cardShape.Footer.ToolFormSettings.ContentType = cardShape.ContentItem.ContentType;
                }
                else
                {
                    cardShape.Footer.Remove("ToolFormSettings");
                }
            });
        }

    }
}
