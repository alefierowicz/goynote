using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Markdown;
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
                            .FontColor(Colors.Grey.Darken4)
                            .Bold();

                        if (note.Workspace != null)
                        {
                            col.Item().Text($"Workspace: {note.Workspace.Name}")
                                .FontSize(11)
                                .FontColor(Colors.Grey.Medium);
                        }

                        col.Item().PaddingVertical(10).Border(1).BorderColor(Colors.Grey.Lighten2);

                        col.Item().Markdown(note.Content);
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}
