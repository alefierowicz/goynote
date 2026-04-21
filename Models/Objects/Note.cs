using System.ComponentModel.DataAnnotations;

namespace goynote.Models.Objects
{
    public class Note : DatabaseEntry
    {
        [Required]
        [MaxLength(30)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(1024)]
        public string Content { get; set; } = string.Empty;

        [Required]
        public Guid WorkspaceID { get; set; }

        [Required]
        public Workspace Workspace { get; set; } = null!;

        public Note() : base() { }
    }
}