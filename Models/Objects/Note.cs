using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

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

        [ValidateNever]
        public Workspace Workspace { get; set; } = null!;

        public ICollection<Attachment>? Attachments { get; set; } = new List<Attachment>();
        public Note() : base() { }
    }
}