using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EduResolve.Models
{
    public class Complaint
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; }

        [Required]
        [MaxLength(4000)]
        public string Description { get; set; }

        [Required]
        [MaxLength(100)]
        public string Category { get; set; }

        public ComplaintStatus Status { get; set; } = ComplaintStatus.New;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? EscalatedAt { get; set; }

        public string SubmittedByUserId { get; set; }
        public string AssignedToUserId { get; set; }
        public bool IsEscalated { get; set; }

        public ApplicationUser SubmittedBy { get; set; }
        public ApplicationUser AssignedTo { get; set; }
        public ICollection<ComplaintAttachment> Attachments { get; set; }
        public ICollection<Comment> Comments { get; set; }
    }
}
