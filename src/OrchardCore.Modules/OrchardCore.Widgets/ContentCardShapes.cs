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
                        // ContentCard_Edit__[CollectionType] e.g. ContentCard_Edit__Flow, ContentCard_Edit__Bag, ContentCard_Edit__List
                        //TODO: Rename Widget-Flow.Edit.cshtml and Widget-Bag.Edit.cshtml to ContentCard-Flow.Edit.cshtml and ContentCard-Bag.Edit.cshtml
                        // and then change following line Widget_Edit__ to {CONTENT_CARD_EDIT}
                        contentCardEditor.Metadata.Alternates.Add($"Widget_Edit__{collectionType}");

                        //Define edit card shape per content type 
                        // ContentCard_Edit__[ContentType] e.g. ContentCard_Edit__Paragraph, ContentCard_Edit__Form, ContentCard_Edit__Input
                        contentCardEditor.Metadata.Alternates.Add($"{CONTENT_CARD_EDIT}__{collectionType}__{contentType}");

                        //If we have Parent Content Type, 
                        if (!string.IsNullOrWhiteSpace(parentContentType))
                        {
                            //Define edit card shape for all child  in collection per parent content type
                            // ContentCard_Edit__[ParentContentType]__[CollectionType] e.g. ContentCard_Edit__Page_Flow, ContentCard_Edit__Form_Flow, ContentCard_Edit__Services__Bag
                            contentCardEditor.Metadata.Alternates.Add($"{CONTENT_CARD_EDIT}__{parentContentType}__{collectionType}");

                            //Define edit card shape for selected child  with specific type per parent content type
                            // ContentCard_Edit__[ParentContentType]__[ContentType] e.g. ContentCard_Edit__Page_Form, ContentCard_Edit__Form_Label, ContentCard_Edit__LandingPage__Service
                            contentCardEditor.Metadata.Alternates.Add($"{CONTENT_CARD_EDIT}__{parentContentType}__{contentType}");

                            //Define edit card shape for selected child  with specific type per parent content type for given collection
                            // ContentCard_Edit__[ParentContentType]__[CollectionType]__[ContentType] e.g. ContentCard_Edit__LandingPage__Flow__Service,
                            // ContentCard_Edit__LandingPage__Bag__Service, ContentCard_Edit__Form__Flow_Label
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
                    string collectionType = context.Shape.ChildContent.CollectionShapeType as string;
                    string contentType = context.Shape.ChildContent.ContentTypeValue as string;
                    string parentContentType = context.Shape.ChildContent.ParentContentType;
                    string namedPart = context.Shape.ChildContent.CollectionPartName;

                    //Define Frame card shape per collection type
                    // ContentCard_Frame__[CollectionType] e.g. ContentCard_Frame__Flow, ContentCard_Frame__Bag, ContentCard_Frame__List
                    contentCardFrame.Metadata.Alternates.Add($"{CONTENT_CARD_FRAME}__{collectionType}");

                    //Define Frame card shape per content type 
                    // ContentCard_Frame__[ContentType] e.g. ContentCard_Frame__Paragraph, ContentCard_Frame__Form, ContentCard_Frame__Input
                    contentCardFrame.Metadata.Alternates.Add($"{CONTENT_CARD_FRAME}__{collectionType}__{contentType}");

                    if (!string.IsNullOrWhiteSpace(parentContentType))
                    {                                             
                        //Define frame card shape for children per parent content type for given collection
                        // ContentCard_Frame__[ParentContentType]__[CollectionType] e.g. ContentCard_Frame__LandingPage__Flow,
                        // ContentCard_Frame__LandingPage__Bag,ContentCard_Frame__Form__Flow
                        contentCardFrame.Metadata.Alternates.Add($"{CONTENT_CARD_FRAME}__{parentContentType}__{collectionType}");

                        //Define frame card shape for child with specific type per parent content type
                        // ContentCard_Frame__[ContentType]__[ContentType] e.g. ContentCard_Frame__Page_Form, ContentCard_Frame__Form_Label
                        contentCardFrame.Metadata.Alternates.Add($"{CONTENT_CARD_FRAME}__{parentContentType}__{contentType}");


                        //Define edit frame shape for selected child  with specific type per parent content type for given collection
                        // ContentCard_Frame__[ParentContentType]__[CollectionType]__[ContentType] e.g. ContentCard_Frame__LandingPage__Flow__Service,
                        // ContentCard_Frame__LandingPage__Bag__Service,ContentCard_Frame__Form__Flow_Label
                        contentCardFrame.Metadata.Alternates.Add($"{CONTENT_CARD_FRAME}__{parentContentType}__{collectionType}__{contentType}");


                        if (!string.IsNullOrWhiteSpace(namedPart))
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
                    contentCardEditorFields.Metadata.Alternates.Add($"{collectionType}Part_Fields_Edit");
               });
        }
    }
}
