using System;
using OrchardCore.DisplayManagement.Descriptors;

namespace OrchardCore.Widgets
{
    public class ContentCardShapes : IShapeTableProvider
    {
        //Card Shape
        private const string ContentCardEdit = "ContentCard_Edit";

        //Frame shape
        private const string ContentCardFrame = "ContentCard_Frame";

        //Card Editor Fields
        private const string ContentCardFieldsEdit = "ContentCard_Fields_Edit";

        public void Discover(ShapeTableBuilder builder)
        {
            builder.Describe(ContentCardEdit)
                .OnDisplaying(context =>
                {
                    //Defines Edit Alternates for the Content Item being edited.
                    dynamic contentCardEditor = context.Shape;
                    string collectionType = contentCardEditor.CollectionShapeType;
                    string contentType = contentCardEditor.ContentTypeValue;
                    string parentContentType = contentCardEditor.ParentContentType;
                    string namedPart = contentCardEditor.CollectionPartName;
                    if (contentCardEditor.BuildEditor == true)
                    {
                        //Define edit card shape per collection type
                        //ContentCard_Edit__[CollectionType]
                        //e.g. ContentCard_Edit__FlowPart, ContentCard_Edit__BagPart, ContentCard_Edit__WidgetsListPart
                        contentCardEditor.Metadata.Alternates.Add($"{ContentCardEdit}__{collectionType}");

                        //Define edit card shape per content type
                        // ContentCard_Edit__[ContentType] e.g. ContentCard_Edit__Paragraph, ContentCard_Edit__Form, ContentCard_Edit__Input
                        contentCardEditor.Metadata.Alternates.Add($"{ContentCardEdit}__{contentType}");

                        //Define edit card shape per content type per collection type
                        //ContentCard_Edit__[CollectionType]__[ContentType]
                        //e.g. ContentCard_Edit__FlowPart__Paragraph, ContentCard_Edit__BagPart__Form, ContentCard_Edit__FlowPart__Input
                        contentCardEditor.Metadata.Alternates.Add($"{ContentCardEdit}__{collectionType}__{contentType}");

                        //If we have Parent Content Type,
                        if (!string.IsNullOrWhiteSpace(parentContentType))
                        {
                            //Define edit card shape for all child  in collection per parent content type
                            //ContentCard_Edit__[ParentContentType]__[CollectionType]
                            //e.g. ContentCard_Edit__Page__FlowPart, ContentCard_Edit__Form__FlowPart, ContentCard_Edit__Services__BagPart
                            contentCardEditor.Metadata.Alternates.Add($"{ContentCardEdit}__{parentContentType}__{collectionType}");

                            //Define edit card shape for selected child  with specific type per parent content type
                            //ContentCard_Edit__[ParentContentType]__[ContentType]
                            //e.g. ContentCard_Edit__Page__Form, ContentCard_Edit__Form__Label, ContentCard_Edit__LandingPage__Service
                            contentCardEditor.Metadata.Alternates.Add($"{ContentCardEdit}__{parentContentType}__{contentType}");

                            //Define edit card shape for selected child  with specific type per parent content type for given collection
                            //ContentCard_Edit__[ParentContentType]__[CollectionType]__[ContentType] e.g. ContentCard_Edit__LandingPage__FlowPart__Service,
                            //ContentCard_Edit__LandingPage__BagPart__Service, ContentCard_Edit__Form__FlowPart__Label
                            contentCardEditor.Metadata.Alternates.Add($"{ContentCardEdit}__{parentContentType}__{collectionType}__{contentType}");

                            if (!string.IsNullOrWhiteSpace(namedPart) && !(namedPart.Equals(collectionType)))
                            {
                                //Define edit card shape for selected child  with specific type and partname per parent content type
                                //ContentCard_Edit__[ParentContentType]__[PartName]
                                //e.g. ContentCard_Edit__Grid__LeftColumn, ContentCard_Edit__LandingPage__Services
                                contentCardEditor.Metadata.Alternates.Add($"{ContentCardEdit}__{parentContentType}__{namedPart}");

                                //Define edit card shape for selected child  with specific type and partname per parent content type
                                //ContentCard_Edit__[ContentType]__[ContentType]
                                //e.g. ContentCard_Edit__Grid__LeftColumn__Client, ContentCard_Edit__LandingPage__Services__Service
                                contentCardEditor.Metadata.Alternates.Add($"{ContentCardEdit}__{parentContentType}__{namedPart}__{contentType}");
                            }
                        }
                    }
                });

            builder.Describe(ContentCardFrame)
                .OnDisplaying(context =>
                {
                    // Alternates for Outer Frame of ContentCard
                    dynamic contentCardFrame = context.Shape;
                    string collectionType = contentCardFrame.ChildContent.CollectionShapeType;
                    string contentType = contentCardFrame.ChildContent.ContentTypeValue as string;
                    string parentContentType = contentCardFrame.ChildContent.ParentContentType;
                    string namedPart = contentCardFrame.ChildContent.CollectionPartName;

                    //Define Frame card shape per collection type
                    //ContentCard_Frame__[CollectionType]
                    //e.g. ContentCard_Frame__FlowPart, ContentCard_Frame__BagPart, ContentCard_Frame__WidgetsListPart
                    contentCardFrame.Metadata.Alternates.Add($"{ContentCardFrame}__{collectionType}");

                    //Define Frame card shape per content type
                    //ContentCard_Frame__[ContentType]
                    //e.g. ContentCard_Frame__Paragraph, ContentCard_Frame__Form, ContentCard_Frame__Input
                    contentCardFrame.Metadata.Alternates.Add($"{ContentCardFrame}__{contentType}");

                    //Define Frame card shape per content type per collection type
                    //ContentCard_Frame__[CollectionType]__[ContentType]
                    //e.g. ContentCard_Frame__FlowPart__Paragraph, ContentCard_Frame__BagPart__Form, ContentCard_Frame__FlowPart__Input
                    contentCardFrame.Metadata.Alternates.Add($"{ContentCardFrame}__{collectionType}__{contentType}");

                    if (!String.IsNullOrWhiteSpace(parentContentType))
                    {
                        //Define frame card shape for children per parent content type for given collection
                        //ContentCard_Frame__[ParentContentType]__[CollectionType]
                        //e.g. ContentCard_Frame__Page__FlowPart, ContentCard_Frame__LandingPage__BagPart, ContentCard_Frame__Form__FlowPart
                        contentCardFrame.Metadata.Alternates.Add($"{ContentCardFrame}__{parentContentType}__{collectionType}");

                        //Define frame card shape for child with specific type per parent content type
                        //ContentCard_Frame__[ParentContentType]__[ContentType]
                        //e.g. ContentCard_Frame__Page__Form, ContentCard_Frame__Form__Label
                        contentCardFrame.Metadata.Alternates.Add($"{ContentCardFrame}__{parentContentType}__{contentType}");

                        //Define edit frame shape for selected child  with specific type per parent content type for given collection
                        //ContentCard_Frame__[ParentContentType]__[CollectionType]__[ContentType]
                        //e.g. ContentCard_Frame__Page__FlowPart__Container, ContentCard_Frame__LandingPage__BagPart__Service, ContentCard_Frame__Form__FlowPart__Label
                        contentCardFrame.Metadata.Alternates.Add($"{ContentCardFrame}__{parentContentType}__{collectionType}__{contentType}");

                        if (!String.IsNullOrWhiteSpace(namedPart) && !namedPart.Equals(collectionType))
                        {
                            //Define frame card shape for child  with specific partname and parent content type
                            //ContentCard_Frame__[ParentContentType]__[PartName]
                            //e.g. ContentCard_Frame__Grid__LeftColumn, ContentCard_Frame__LandingPage__Services
                            contentCardFrame.Metadata.Alternates.Add($"{ContentCardFrame}__{parentContentType}__{namedPart}");

                            //Define edit card shape for selected child with specific type per parent content type
                            //ContentCard_Frame__[ParentContentType]__[NamedPart]__[ContentType]
                            //e.g. ContentCard_Frame__Grid__LeftColumn__Client, ContentCard_Frame__LandingPage__Clients__Client
                            contentCardFrame.Metadata.Alternates.Add($"{ContentCardFrame}__{parentContentType}__{namedPart}__{contentType}");
                        }
                    }
                });

            builder.Describe(ContentCardFieldsEdit)
               .OnDisplaying(context =>
               {
                   dynamic contentCardEditorFields = context.Shape;
                   string collectionType = contentCardEditorFields.CardShape.CollectionShapeType as string;
                   contentCardEditorFields.Metadata.Alternates.Add($"{collectionType}_Fields_Edit");
               });
        }
    }
}
