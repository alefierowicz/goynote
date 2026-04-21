using System.ComponentModel.DataAnnotations;

namespace goynote.Models.Objects
{
    public abstract class DatabaseEntry
    {
        [Key]
        [Required]
        public Guid ID { get; set; } = Guid.NewGuid();
    }
}
