using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EduResolve.ViewModels
{
    public class ComplaintCreateViewModel
    {
        [Required]
        [MaxLength(150)]
        public string Title { get; set; }

        [Required]
        [MaxLength(4000)]
        public string Description { get; set; }

        [Required]
        [MaxLength(100)]
        public string Category { get; set; }

        [Required]
        [Display(Name = "Assign To")]
        public string RecipientUserId { get; set; }

        public IEnumerable<SelectListItem> RecipientOptions { get; set; }
        public IEnumerable<SelectListItem> CategoryOptions { get; set; }

        [Display(Name = "Attachment")]
        public IFormFile Attachment { get; set; }
    }
}
