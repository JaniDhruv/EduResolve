using System.ComponentModel.DataAnnotations;

namespace EduResolve.Models
{
    public class ComplaintAttachment
    {
        public int Id { get; set; }

        [Required]
        public string FilePath { get; set; }

        public int ComplaintId { get; set; }
        public Complaint Complaint { get; set; }
    }
}
