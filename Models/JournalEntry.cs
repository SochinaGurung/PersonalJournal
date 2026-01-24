using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Coursework.Models
{
    public class JournalEntry
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public string PrimaryMood { get; set; } = string.Empty;
        
        public string SecondaryMoods { get; set; } = string.Empty; // JSON array or comma-separated

        public string Category { get; set; } = "Personal Journal";

        public string Tags { get; set; } = string.Empty; // JSON array or comma-separated

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public bool IsFavorite { get; set; } = false;
    }
}
