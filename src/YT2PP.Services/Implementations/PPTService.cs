using YT2PP.Services.Interfaces;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using D = DocumentFormat.OpenXml.Drawing;
using P = DocumentFormat.OpenXml.Presentation;
using A = DocumentFormat.OpenXml.Drawing;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace YT2PP.Services.Implementations
{
    public class PPTService : IPPTService
    {
        public void CreatePresentation(string outputFilePath, string framesDirectory)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath));

            using (PresentationDocument presentationDocument = PresentationDocument.Create(outputFilePath, PresentationDocumentType.Presentation))
            {
                PresentationPart presentationPart = presentationDocument.AddPresentationPart();

                // Add required presentation properties
                presentationPart.AddNewPart<PresentationPropertiesPart>();

                // Create and configure SlideMasterPart with theme
                SlideMasterPart slideMasterPart = presentationPart.AddNewPart<SlideMasterPart>();
                CreateSlideMasterPart(slideMasterPart);

                // Add theme part
                ThemePart themePart = slideMasterPart.AddNewPart<ThemePart>();
                CreateTheme(themePart);

                // Create and configure SlideLayoutPart
                SlideLayoutPart slideLayoutPart = slideMasterPart.AddNewPart<SlideLayoutPart>();
                CreateSlideLayoutPart(slideLayoutPart);

                // Set the slide layout relationship ID
                slideMasterPart.AddPart(slideLayoutPart);

                // Set up presentation structure with slide size
                presentationPart.Presentation = new P.Presentation(
                    new P.SlideMasterIdList(
                        new P.SlideMasterId() { Id = 2147483648U, RelationshipId = presentationPart.GetIdOfPart(slideMasterPart) }
                    ),
                    new P.SlideIdList(),
                    new P.SlideSize() { 
                        Cx = 9144000, 
                        Cy = 6858000, 
                        Type = SlideSizeValues.Screen16x9 
                    },
                    new P.NotesSize() { 
                        Cx = 6858000, 
                        Cy = 9144000 
                    },
                    new P.DefaultTextStyle()
                );

                // Process each image
                uint slideId = 256;
                string[] pngFiles = Directory.GetFiles(framesDirectory, "*.png").OrderBy(f => f).ToArray();

                if (pngFiles.Length == 0)
                {
                    throw new InvalidOperationException("No PNG files found in the specified directory.");
                }

                foreach (var pngFile in pngFiles)
                {
                    using (FileStream imageStream = new FileStream(pngFile, FileMode.Open, FileAccess.Read))
                    {
                        SlidePart slidePart = presentationPart.AddNewPart<SlidePart>();
                        CreateSlidePart(slidePart, slideLayoutPart);

                        // Add image to slide
                        ImagePart imagePart = slidePart.AddImagePart(ImagePartType.Png);
                        imagePart.FeedData(imageStream);
                        
                        AddImageToSlide(slidePart, slidePart.GetIdOfPart(imagePart));

                        // Add to slide list
                        presentationPart.Presentation.SlideIdList.AppendChild(
                            new P.SlideId() { 
                                Id = slideId++, 
                                RelationshipId = presentationPart.GetIdOfPart(slidePart) 
                            }
                        );
                    }
                }

                // Save the presentation
                presentationPart.Presentation.Save();
            }
        }

        private void CreateSlideMasterPart(SlideMasterPart slideMasterPart)
        {
            var slideMaster = new P.SlideMaster(
                new P.CommonSlideData(
                    new P.ShapeTree(
                        new P.NonVisualGroupShapeProperties(
                            new P.NonVisualDrawingProperties() { Id = 1U, Name = "" },
                            new P.NonVisualGroupShapeDrawingProperties(),
                            new P.ApplicationNonVisualDrawingProperties()),
                        new P.GroupShapeProperties()
                    )
                ),
                new P.ColorMap() {
                    Background1 = D.ColorSchemeIndexValues.Light1,
                    Text1 = D.ColorSchemeIndexValues.Dark1,
                    Background2 = D.ColorSchemeIndexValues.Light2,
                    Text2 = D.ColorSchemeIndexValues.Dark2,
                    Accent1 = D.ColorSchemeIndexValues.Accent1,
                    Accent2 = D.ColorSchemeIndexValues.Accent2,
                    Accent3 = D.ColorSchemeIndexValues.Accent3,
                    Accent4 = D.ColorSchemeIndexValues.Accent4,
                    Accent5 = D.ColorSchemeIndexValues.Accent5,
                    Accent6 = D.ColorSchemeIndexValues.Accent6,
                    Hyperlink = D.ColorSchemeIndexValues.Hyperlink,
                    FollowedHyperlink = D.ColorSchemeIndexValues.FollowedHyperlink
                }
            );
            slideMasterPart.SlideMaster = slideMaster;
        }

        private void CreateSlideLayoutPart(SlideLayoutPart slideLayoutPart)
        {
            var slideLayout = new P.SlideLayout(
                new P.CommonSlideData(
                    new P.ShapeTree(
                        new P.NonVisualGroupShapeProperties(
                            new P.NonVisualDrawingProperties() { Id = 1U, Name = "" },
                            new P.NonVisualGroupShapeDrawingProperties(),
                            new P.ApplicationNonVisualDrawingProperties()
                        ),
                        new P.GroupShapeProperties()
                    )
                )
            );
            slideLayoutPart.SlideLayout = slideLayout;
        }

        private void CreateSlidePart(SlidePart slidePart, SlideLayoutPart slideLayoutPart)
        {
            var slide = new P.Slide(
                new P.CommonSlideData(
                    new P.ShapeTree(
                        new P.NonVisualGroupShapeProperties(
                            new P.NonVisualDrawingProperties() { Id = 1U, Name = "" },
                            new P.NonVisualGroupShapeDrawingProperties(),
                            new P.ApplicationNonVisualDrawingProperties()
                        ),
                        new P.GroupShapeProperties()
                    )
                )
            );
            slidePart.Slide = slide;
            slidePart.AddPart(slideLayoutPart);
        }

        private static void AddImageToSlide(SlidePart slidePart, string relationshipId)
        {
            if (slidePart.Slide?.CommonSlideData?.ShapeTree == null)
            {
                throw new InvalidOperationException("Slide structure is not properly initialized.");
            }

            var picture = new P.Picture(
                new P.NonVisualPictureProperties(
                    new P.NonVisualDrawingProperties() { Id = 2U, Name = "Picture" },
                    new P.NonVisualPictureDrawingProperties(new A.PictureLocks() { NoChangeAspect = true }),
                    new P.ApplicationNonVisualDrawingProperties()
                ),
                new P.BlipFill(
                    new A.Blip() { Embed = relationshipId },
                    new A.Stretch(new A.FillRectangle())
                ),
                new P.ShapeProperties(
                    new A.Transform2D(
                        new A.Offset() { X = 0L, Y = 0L },
                        new A.Extents() { Cx = 9144000L, Cy = 6858000L }
                    ),
                    new A.PresetGeometry(new A.AdjustValueList()) { Preset = A.ShapeTypeValues.Rectangle }
                )
            );

            slidePart.Slide.CommonSlideData.ShapeTree.AppendChild(picture);
        }

        private void CreateTheme(ThemePart themePart)
        {
            var theme = new D.Theme() { Name = "Default Theme" };
            var colorScheme = new D.ColorScheme() { Name = "Office" };
            
            // Add color scheme elements
            colorScheme.AppendChild(new D.Dark1Color(new D.SystemColor() { Val = D.SystemColorValues.WindowText, LastColor = "000000" }));
            colorScheme.AppendChild(new D.Light1Color(new D.SystemColor() { Val = D.SystemColorValues.Window, LastColor = "FFFFFF" }));
            colorScheme.AppendChild(new D.Dark2Color(new D.RgbColorModelHex() { Val = "1F497D" }));
            colorScheme.AppendChild(new D.Light2Color(new D.RgbColorModelHex() { Val = "EEECE1" }));
            colorScheme.AppendChild(new D.Accent1Color(new D.RgbColorModelHex() { Val = "4F81BD" }));
            colorScheme.AppendChild(new D.Accent2Color(new D.RgbColorModelHex() { Val = "C0504D" }));
            colorScheme.AppendChild(new D.Accent3Color(new D.RgbColorModelHex() { Val = "9BBB59" }));
            colorScheme.AppendChild(new D.Accent4Color(new D.RgbColorModelHex() { Val = "8064A2" }));
            colorScheme.AppendChild(new D.Accent5Color(new D.RgbColorModelHex() { Val = "4BACC6" }));
            colorScheme.AppendChild(new D.Accent6Color(new D.RgbColorModelHex() { Val = "F79646" }));
            colorScheme.AppendChild(new D.Hyperlink(new D.RgbColorModelHex() { Val = "0000FF" }));
            colorScheme.AppendChild(new D.FollowedHyperlinkColor(new D.RgbColorModelHex() { Val = "800080" }));

            // Add font scheme
            var fontScheme = new D.FontScheme() { Name = "Office" };
            
            var majorFont = new D.MajorFont();
            majorFont.AppendChild(new D.LatinFont() { Typeface = "Calibri Light", Panose = "020F0302020204030204" });
            majorFont.AppendChild(new D.EastAsianFont() { Typeface = "" });
            majorFont.AppendChild(new D.ComplexScriptFont() { Typeface = "" });

            var minorFont = new D.MinorFont();
            minorFont.AppendChild(new D.LatinFont() { Typeface = "Calibri", Panose = "020F0502020204030204" });
            minorFont.AppendChild(new D.EastAsianFont() { Typeface = "" });
            minorFont.AppendChild(new D.ComplexScriptFont() { Typeface = "" });

            fontScheme.AppendChild(majorFont);
            fontScheme.AppendChild(minorFont);

            // Add format scheme
            var formatScheme = new D.FormatScheme() { Name = "Office" };
            
            var fillStyleList = new D.FillStyleList();
            fillStyleList.AppendChild(new D.SolidFill() { 
                SchemeColor = new D.SchemeColor() { Val = D.SchemeColorValues.PhColor }
            });
            
            var lineStyleList = new D.LineStyleList();
            lineStyleList.AppendChild(new D.Outline() {
                Width = 9525,
                CapType = D.LineCapValues.Flat,
                CompoundLineType = D.CompoundLineValues.Single
            });

            formatScheme.AppendChild(fillStyleList);
            formatScheme.AppendChild(lineStyleList);
            formatScheme.AppendChild(new D.EffectStyleList());
            formatScheme.AppendChild(new D.BackgroundFillStyleList());

            // Construct theme elements
            var themeElements = new D.ThemeElements();
            themeElements.AppendChild(colorScheme);
            themeElements.AppendChild(fontScheme);
            themeElements.AppendChild(formatScheme);

            theme.AppendChild(themeElements);
            theme.AppendChild(new D.ObjectDefaults());
            theme.AppendChild(new D.ExtraColorSchemeList());

            themePart.Theme = theme;
        }
    }
}
