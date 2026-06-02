using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using goynote.Models.System;

namespace goynote.Models.Objects
{
    public class WorkspaceMember : DatabaseEntry
    {
        [Required]
        public Guid WorkspaceID { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ValidateNever]
        public Workspace Workspace { get; set; } = null!;

        [ValidateNever]
        public ApplicationUser User { get; set; } = null!;

        public WorkspaceMember() : base()
        {
        }
    }
}
