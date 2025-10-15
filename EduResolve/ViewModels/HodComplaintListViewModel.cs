using System.Collections.Generic;
using EduResolve.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EduResolve.ViewModels
{
    public class HodComplaintListViewModel
    {
        public IEnumerable<Complaint> Complaints { get; set; }
        public string SelectedStatus { get; set; }
        public string SelectedTeacherId { get; set; }
        public string SelectedStudentId { get; set; }

        public IEnumerable<SelectListItem> StatusOptions { get; set; }
        public IEnumerable<SelectListItem> TeacherOptions { get; set; }
        public IEnumerable<SelectListItem> StudentOptions { get; set; }
    }
}
