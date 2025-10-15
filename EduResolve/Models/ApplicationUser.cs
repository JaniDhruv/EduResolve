using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace EduResolve.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? DepartmentId { get; set; }

        public Department Department { get; set; }
        public ICollection<Complaint> SubmittedComplaints { get; set; }
        public ICollection<Complaint> AssignedComplaints { get; set; }
        public ICollection<Comment> Comments { get; set; }
    }
}
