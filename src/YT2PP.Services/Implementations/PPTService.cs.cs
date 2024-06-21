using YT2PP.Services.Interfaces;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using D = DocumentFormat.OpenXml.Drawing; // Alias for Drawing namespace
using P = DocumentFormat.OpenXml.Presentation; // Alias for Presentation namespace
using A = DocumentFormat.OpenXml.Drawing;
using System;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml.Drawing;
using System.Collections.Generic;


namespace YT2PP.Services.Implementations
{
    public class PPTService : IPPTService
    {
        public void CreatePresentation(string outputFilePath, string framesDirectory)
        {
            using (PresentationDocument presentationDocument = PresentationDocument.Create(outputFilePath, PresentationDocumentType.Presentation))
            {
                // Create the presentation part and add the presentation to the presentation part
                PresentationPart presentationPart = presentationDocument.AddPresentationPart();
                presentationPart.Presentation = new P.Presentation();

                // Create the slide master part
                SlideMasterPart slideMasterPart = presentationPart.AddNewPart<SlideMasterPart>("rId1");
                slideMasterPart.SlideMaster = new P.SlideMaster(
                    new P.CommonSlideData(new P.ShapeTree(
                        new P.NonVisualGroupShapeProperties(
                            new P.NonVisualDrawingProperties() { Id = (UInt32Value)1U, Name = "" },
                            new P.NonVisualGroupShapeDrawingProperties(),
                            new P.ApplicationNonVisualDrawingProperties()),
                        new P.GroupShapeProperties(new A.TransformGroup()),
                        new P.Shape(
                            new P.NonVisualShapeProperties(
                                new P.NonVisualDrawingProperties() { Id = (UInt32Value)2U, Name = "Title" },
                                new P.NonVisualShapeDrawingProperties(new A.ShapeLocks() { NoGrouping = true }),
                                new P.ApplicationNonVisualDrawingProperties(new P.PlaceholderShape())),
                            new P.ShapeProperties(),
                            new P.TextBody(
                                new A.BodyProperties(),
                                new A.ListStyle(),
                                new A.Paragraph(new A.Run(new A.Text() { Text = "Title" }))))))
                );

                // Create the slide layout part
                SlideLayoutPart slideLayoutPart = slideMasterPart.AddNewPart<SlideLayoutPart>("rId2");
                slideLayoutPart.SlideLayout = new P.SlideLayout(new P.CommonSlideData(new P.ShapeTree()));

                // Create the slide master ID list
                presentationPart.Presentation.SlideMasterIdList = new P.SlideMasterIdList();
                P.SlideMasterId slideMasterId = new P.SlideMasterId();
                Dictionary<string, uint> relationshipIdMapping = new Dictionary<string, uint>();

                // Method to get or add a relationship ID
                uint GetOrAddNumericId(string relationshipId)
                {
                    if (!relationshipIdMapping.TryGetValue(relationshipId, out uint numericId))
                    {
                        numericId = (uint)relationshipIdMapping.Count + 1; // Generate a new numeric ID
                        relationshipIdMapping[relationshipId] = numericId;
                    }
                    return numericId;
                }

                // Usage
                string relationshipId = presentationPart.GetIdOfPart(slideMasterPart);
                slideMasterId.Id = GetOrAddNumericId(relationshipId);

                presentationPart.Presentation.SlideMasterIdList.Append(slideMasterId);

                // Slide ID counter
                uint slideId = 256;

                // Get all PNG files from the framesDirectory
                string[] pngFiles = Directory.GetFiles(framesDirectory, "*.png");

                foreach (var pngFile in pngFiles)
                {
                    // Create the slide part
                    SlidePart slidePart = presentationPart.AddNewPart<SlidePart>();
                    slidePart.Slide = new P.Slide(new P.CommonSlideData(new P.ShapeTree(
                        new P.NonVisualGroupShapeProperties(
                            new P.NonVisualDrawingProperties() { Id = (UInt32Value)1U, Name = "" },
                            new P.NonVisualGroupShapeDrawingProperties(),
                            new P.ApplicationNonVisualDrawingProperties()),
                        new P.GroupShapeProperties(new A.TransformGroup()))));

                    // Add image to slide
                    ImagePart imagePart = slidePart.AddImagePart(ImagePartType.Png);
                    using (FileStream stream = new FileStream(pngFile, FileMode.Open))
                    {
                        imagePart.FeedData(stream);
                    }

                    // Define the image properties
                    AddImageToSlide(slidePart, slidePart.GetIdOfPart(imagePart));

                    // Associate the slide layout with the slide
                    slidePart.AddPart(slideLayoutPart);

                    // Create the slide ID and append to the presentation
                    P.SlideIdList slideIdList = presentationPart.Presentation.SlideIdList ?? new P.SlideIdList();
                    P.SlideId id = new P.SlideId() { Id = slideId++, RelationshipId = presentationPart.GetIdOfPart(slidePart) };
                    slideIdList.Append(id);
                    presentationPart.Presentation.SlideIdList = slideIdList;
                }

                // Save the presentation
                presentationPart.Presentation.Save();
            }
        }

        private static void AddImageToSlide(SlidePart slidePart, string relationshipId)
        {
            // Create a new picture element
            var picture = new P.Picture(
                new P.NonVisualPictureProperties(
                    new P.NonVisualDrawingProperties() { Id = (UInt32Value)4U, Name = "Picture 1" },
                    new P.NonVisualPictureDrawingProperties(new A.PictureLocks() { NoChangeAspect = true }),
                    new P.ApplicationNonVisualDrawingProperties()),
                new P.BlipFill(
                    new A.Blip() { Embed = relationshipId },
                    new A.Stretch(new A.FillRectangle())),
                new P.ShapeProperties(
                    new A.Transform2D(
                        new A.Offset() { X = 0L, Y = 0L },
                        new A.Extents() { Cx = 9900000L, Cy = 7920000L }), // Adjusted for full slide size
                    new A.PresetGeometry(new A.AdjustValueList()) { Preset = A.ShapeTypeValues.Rectangle }));

            // Add the picture as the first element in the shape tree
            slidePart.Slide.CommonSlideData.ShapeTree.InsertAt(picture, 0);
        }
    }
}
