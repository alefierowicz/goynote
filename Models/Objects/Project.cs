using System.ComponentModel.DataAnnotations;

namespace goynote.Models.Objects
{
    public class Project : DatabaseEntry
    {
        [Required]
        [MaxLength(30)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(256)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public Guid WorkspaceID { get; set; }

        [Required]
        public Workspace Workspace { get; set; } = null!;

        public ICollection<ProjectSpecificNote> PSNotes { get; set; } = new List<ProjectSpecificNote>();

        public Project() : base() { }
    }
}