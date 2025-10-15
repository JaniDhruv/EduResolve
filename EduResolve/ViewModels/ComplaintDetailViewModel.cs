using System.Collections.Generic;
using EduResolve.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EduResolve.ViewModels
{
    public class ComplaintDetailViewModel
    {
        public Complaint Complaint { get; set; }
        public IEnumerable<Comment> Comments { get; set; }
        public IEnumerable<ComplaintAttachment> Attachments { get; set; }

        public bool CanUpdateStatus { get; set; }
        public IEnumerable<SelectListItem> StatusOptions { get; set; }
        public string NewComment { get; set; }
    }
}
