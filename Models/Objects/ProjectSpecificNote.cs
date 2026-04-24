using System.ComponentModel.DataAnnotations;

namespace goynote.Models.Objects
{
    public class ProjectSpecificNote : Note
    {
        [Required]
        public Project? Project { get; set; }

        [Required]
        public Guid ProjectID { get; set; }
    }
}
