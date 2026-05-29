using System.Text;
using goynote.Models.Objects;

namespace goynote.Models.System
{
    public class MarkdownService
    {
        public byte[] GenerateNoteMarkdown(Note note)
        {
            var markdown = new StringBuilder();

            markdown.AppendLine($"# {note.Name}");
            markdown.AppendLine();

            if (note.Workspace != null)
            {
                markdown.AppendLine($"**Workspace:** {note.Workspace.Name}");
                markdown.AppendLine();
            }

            markdown.AppendLine("---");
            markdown.AppendLine();

            markdown.AppendLine(note.Content);

            return Encoding.UTF8.GetBytes(markdown.ToString());
        }
    }
}
