using System.ComponentModel.DataAnnotations;

namespace goynote.Models.Objects
{
    public class Workspace : DatabaseEntry
    {
        [Required]
        [MaxLength(30)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(256)]
        public string? Description { get; set; }

        public ICollection<Note> Notes { get; set; } = new List<Note>();
        public ICollection<Project> Projects { get; set; } = new List<Project>();

        public Workspace() : base()
        {
        }
    }
}