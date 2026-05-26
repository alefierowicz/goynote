using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using goynote.Models.Objects;

namespace goynote.Models.System
{
    public class PDFService
    {
        public byte[] GenerateNotePDF(Note note)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);

                    page.Content().Column(col =>
                    {
                        col.Item().Text(note.Name)
                            .FontSize(28)
                            .FontColor(Colors.DeepPurple.Darken4)
                            .Bold();

                        // Workspace info
                        if (note.Workspace != null)
                        {
                            col.Item().Text($"Workspace: {note.Workspace.Name}")
                                .FontSize(11)
                                .FontColor(Colors.Grey.Medium);
                        }

                        // Divider
                        col.Item().PaddingVertical(10).Border(1).BorderColor(Colors.Grey.Lighten2);

                        // Content
                        col.Item().Text(note.Content)
                            .FontSize(11)
                            .LineHeight((float)1.5);
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}
