using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace goynote.Models.Objects
{
    public class Attachment : DatabaseEntry
    {
        public Guid? NoteID { get; set; }

        [ValidateNever]
        public Note? Note { get; set; }

        [Required]
        public string S3Key { get; set; } = string.Empty;

        public string FileName { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;
        public int FileSize { get;set;  }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
