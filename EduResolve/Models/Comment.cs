using System;
using System.ComponentModel.DataAnnotations;

namespace EduResolve.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int ComplaintId { get; set; }
        public Complaint Complaint { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
