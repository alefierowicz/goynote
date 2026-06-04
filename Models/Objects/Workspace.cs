using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using goynote.Models.System;

namespace goynote.Models.Objects
{
    public class Workspace : DatabaseEntry
    {
        [Required]
        [MaxLength(30)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(256)]
        public string? Description { get; set; }

        public string OwnerId { get; set; } = string.Empty;

        public bool IsPublic { get; set; } = false;

        [ValidateNever]
        public ApplicationUser Owner { get; set; } = null!;

        public ICollection<Note> Notes { get; set; } = new List<Note>();
        public ICollection<Project> Projects { get; set; } = new List<Project>();
        public ICollection<WorkspaceMember> Members { get; set; } = new List<WorkspaceMember>();

        public Workspace() : base()
        {
        }
    }
}