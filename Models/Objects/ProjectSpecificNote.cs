using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace goynote.Models.Objects
{
    public class ProjectSpecificNote : Note
    {
        [ValidateNever]
        public Project? Project { get; set; }

        [Required]
        public Guid ProjectID { get; set; }
    }
}
