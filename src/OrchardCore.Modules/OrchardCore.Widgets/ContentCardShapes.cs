using System;
using System.Collections.Generic;
using System.Text;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Descriptors;

namespace OrchardCore.Widgets
{
    public class ContentCardShapes : IShapeTableProvider
    {
        //Card Shapes
        //private readonly string CONTENT_CARD = "ContentCard";
        private readonly string CONTENT_CARD_EDIT = "ContentCard_Edit";
        //Wrapper shapes
        private readonly string CONTENT_CARD_FRAME = "ContentCard_Frame";

        //Card Editor Fields
        private readonly string CONTENT_CARD_FIELDS_EDIT = "ContentCard_Fields_Edit";

        public void Discover(ShapeTableBuilder builder)
        {
            builder.Describe(CONTENT_CARD_EDIT)
                .OnDisplaying(context =>
                {
                    //Defines Edit Alternates for the Content Item being edited.

                    dynamic contentCardEditor = context.Shape;
                    string collectionType = context.Shape.CollectionShapeType ;
                    string contentType = context.Shape.ContentTypeValue ;
                    string parentContentType = context.Shape.ParentContentType;
                    string namedPart = context.Shape.CollectionPartName;

                    if (context.Shape.BuildEditor == true)
                    {
                        //Define edit card shape per collection type
                        // ContentCard_Edit__[CollectionType] e.g. ContentCard_Edit__FlowPart, ContentCard_Edit__BagPart, ContentCard_Edit__WidgetsListPart
                        contentCardEditor.Metadata.Alternates.Add($"{CONTENT_CARD_EDIT}__{collectionType}");
                        
                        //Define edit card shape per content type 
                        // ContentCard_Edit__[ContentType] e.g. ContentCard_Edit__Paragraph, ContentCard_Edit__Form, ContentCard_Edit__Input
                        contentCardEditor.Metadata.Alternates.Add($"{CONTENT_CARD_EDIT}__{collectionType}__{contentType}");

                        //If we have Parent Content Type, 
                        if (!string.IsNullOrWhiteSpace(parentContentType))
                        {
                            //Define edit card shape for all child  in collection per parent content type
                            // ContentCard_Edit__[ParentContentType]__[CollectionType] e.g. ContentCard_Edit__Page_FlowPart, ContentCard_Edit__Form_FlowPart, ContentCard_Edit__Services__BagPart
                            contentCardEditor.Metadata.Alternates.Add($"{CONTENT_CARD_EDIT}__{parentContentType}__{collectionType}");

                            //Define edit card shape for selected child  with specific type per parent content type
                            // ContentCard_Edit__[ParentContentType]__[ContentType] e.g. ContentCard_Edit__Page_Form, ContentCard_Edit__Form_Label, ContentCard_Edit__LandingPage__Service
                            contentCardEditor.Metadata.Alternates.Add($"{CONTENT_CARD_EDIT}__{parentContentType}__{contentType}");

                            //Define edit card shape for selected child  with specific type per parent content type for given collection
                            // ContentCard_Edit__[ParentContentType]__[CollectionType]__[ContentType] e.g. ContentCard_Edit__LandingPage__FlowPart__Service,
                            // ContentCard_Edit__LandingPage__BagPart__Service, ContentCard_Edit__Form__FlowPart_Label
                            contentCardEditor.Metadata.Alternates.Add($"{CONTENT_CARD_EDIT}__{parentContentType}__{collectionType}__{contentType}");
                                                      
                            if (!string.IsNullOrWhiteSpace(namedPart) && !(namedPart.Equals(collectionType) ))
                            {
                                //Define edit card shape for selected child  with specific type and partname per parent content type
                                // ContentCard_Edit__[ParentContentType]__[PartName] e.g. ContentCard_Edit__Grid_LeftColumn, ContentCard_Edit__LandingPage__Services
                                contentCardEditor.Metadata.Alternates.Add($"{CONTENT_CARD_EDIT}__{parentContentType}__{namedPart}");

                                //Define edit card shape for selected child  with specific type and partname per parent content type
                                // ContentCard_Edit__[ContentType]__[ContentType] e.g. ContentCard_Edit__Grid_LeftColumn_Client, ContentCard_Edit__LandingPage__Services__Service
                                contentCardEditor.Metadata.Alternates.Add($"{CONTENT_CARD_EDIT}__{parentContentType}__{namedPart}__{contentType}");
                           
                            }
                        }
                    }
                });

            builder.Describe(CONTENT_CARD_FRAME)
                .OnDisplaying(context =>
                {
                    // Alternates for Outer Frame of ContentCard
                    dynamic contentCardFrame = context.Shape;
                    string collectionType = context.Shape.ChildContent.CollectionShapeType;
                    string contentType = context.Shape.ChildContent.ContentTypeValue as string;
                    string parentContentType = context.Shape.ChildContent.ParentContentType;
                    string namedPart = context.Shape.ChildContent.CollectionPartName;

                    //Define Frame card shape per collection type
                    // ContentCard_Frame__[CollectionType] e.g. ContentCard_Frame__FlowPart, ContentCard_Frame__BagPart, ContentCard_Frame__WidgetsListPart
                    contentCardFrame.Metadata.Alternates.Add($"{CONTENT_CARD_FRAME}__{collectionType}");

                    //Define Frame card shape per content type 
                    // ContentCard_Frame__[ContentType] e.g. ContentCard_Frame__Paragraph, ContentCard_Frame__Form, ContentCard_Frame__Input
                    contentCardFrame.Metadata.Alternates.Add($"{CONTENT_CARD_FRAME}__{collectionType}__{contentType}");

                    if (!string.IsNullOrWhiteSpace(parentContentType))
                    {                                             
                        //Define frame card shape for children per parent content type for given collection
                        // ContentCard_Frame__[ParentContentType]__[CollectionType] e.g. ContentCard_Frame__LandingPage__FlowPart,
                        // ContentCard_Frame__LandingPage__BagPart,ContentCard_Frame__Form__FlowPart
                        contentCardFrame.Metadata.Alternates.Add($"{CONTENT_CARD_FRAME}__{parentContentType}__{collectionType}");

                        //Define frame card shape for child with specific type per parent content type
                        // ContentCard_Frame__[ContentType]__[ContentType] e.g. ContentCard_Frame__Page_Form, ContentCard_Frame__Form_Label
                        contentCardFrame.Metadata.Alternates.Add($"{CONTENT_CARD_FRAME}__{parentContentType}__{contentType}");


                        //Define edit frame shape for selected child  with specific type per parent content type for given collection
                        // ContentCard_Frame__[ParentContentType]__[CollectionType]__[ContentType] e.g. ContentCard_Frame__LandingPage__FlowPart__Service,
                        // ContentCard_Frame__LandingPage__BagPart__Service,ContentCard_Frame__Form__FlowPart_Label
                        contentCardFrame.Metadata.Alternates.Add($"{CONTENT_CARD_FRAME}__{parentContentType}__{collectionType}__{contentType}");


                        if (!string.IsNullOrWhiteSpace(namedPart) && !namedPart.Equals(collectionType))
                        {
                            //Define frame card shape for child  with specific partname and parent content type
                            // ContentCard_Frame__[ParentContentType]__[PartName] e.g. ContentCard_Frame__Grid_LeftColumn, ContentCard_Frame__LandingPage__Services
                            contentCardFrame.Metadata.Alternates.Add($"{CONTENT_CARD_FRAME}__{parentContentType}__{namedPart}");

                            //Define edit card shape for selected child with specific type per parent content type
                            // ContentCard_Frame__[ParentContentType]__[NamedPart]__[ContentType] e.g. ContentCard_Frame__Page_ContactForm__Input, ContentCard_Frame__LandingPage__Clients__Client
                            contentCardFrame.Metadata.Alternates.Add($"{CONTENT_CARD_FRAME}__{parentContentType}__{namedPart}__{contentType}");

                        }
                    }

                });

            builder.Describe(CONTENT_CARD_FIELDS_EDIT)
               .OnDisplaying(context =>
               {
                    dynamic contentCardEditorFields = context.Shape;
                    string collectionType = context.Shape.CardShape.CollectionShapeType as string;
                    contentCardEditorFields.Metadata.Alternates.Add($"{collectionType}_Fields_Edit");
               });
        }
    }
}
