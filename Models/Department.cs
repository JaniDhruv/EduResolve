using System.Collections.Generic;

namespace EduResolve.Models
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<ApplicationUser> Users { get; set; }
        public ICollection<Complaint> Complaints { get; set; }
    }
}
