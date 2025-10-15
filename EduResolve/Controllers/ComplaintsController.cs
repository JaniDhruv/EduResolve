using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EduResolve.Data;
using EduResolve.Models;
using EduResolve.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduResolve.Controllers
{
    [Authorize]
    public class ComplaintsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ComplaintsController> _logger;

        public ComplaintsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment,
            ILogger<ComplaintsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string status = null, string origin = null)
        {
            var user = await _userManager.GetUserAsync(User);
            var query = _context.Complaints
                .Include(c => c.SubmittedBy)
                .Include(c => c.AssignedTo)
                .AsQueryable();

            if (User.IsInRole("Student"))
            {
                query = query.Where(c => c.SubmittedByUserId == user.Id);
            }
            else if (User.IsInRole("Teacher"))
            {
                query = query.Where(c => c.AssignedToUserId == user.Id || c.SubmittedByUserId == user.Id);

                if (!string.IsNullOrEmpty(origin))
                {
                    if (string.Equals(origin, "assigned", StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(c => c.AssignedToUserId == user.Id && c.SubmittedByUserId != user.Id);
                    }
                    else if (string.Equals(origin, "submitted", StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(c => c.SubmittedByUserId == user.Id);
                    }
                }
            }
            else if (User.IsInRole("HOD"))
            {
                var departmentId = user.DepartmentId;
                query = query.Where(c =>
                    (c.SubmittedBy != null && c.SubmittedBy.DepartmentId == departmentId)
                    || (c.AssignedTo != null && c.AssignedTo.DepartmentId == departmentId));
            }

            if (!string.IsNullOrEmpty(status) && Enum.TryParse(status, out ComplaintStatus parsedStatus))
            {
                query = query.Where(c => c.Status == parsedStatus);
            }

            var complaints = await query
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            ViewBag.StatusFilter = status;
            ViewBag.StatusOptions = Enum.GetNames(typeof(ComplaintStatus))
                .Select(s => new SelectListItem { Text = s, Value = s, Selected = s == status })
                .ToList();

            if (User.IsInRole("Teacher"))
            {
                ViewBag.OriginFilter = origin;
                ViewBag.OriginOptions = new List<SelectListItem>
                {
                    new SelectListItem { Text = "All", Value = string.Empty, Selected = string.IsNullOrEmpty(origin) },
                    new SelectListItem { Text = "Assigned by Students", Value = "assigned", Selected = string.Equals(origin, "assigned", StringComparison.OrdinalIgnoreCase) },
                    new SelectListItem { Text = "Submitted to HOD", Value = "submitted", Selected = string.Equals(origin, "submitted", StringComparison.OrdinalIgnoreCase) }
                };
            }

            return View(complaints);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            var viewModel = new ComplaintCreateViewModel
            {
                CategoryOptions = GetCategoryOptions(),
                RecipientOptions = await GetRecipientOptionsAsync(user)
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ComplaintCreateViewModel viewModel)
        {
            var user = await _userManager.GetUserAsync(User);

            if (!ModelState.IsValid)
            {
                viewModel.CategoryOptions = GetCategoryOptions();
                viewModel.RecipientOptions = await GetRecipientOptionsAsync(user);
                return View(viewModel);
            }

            var complaint = new Complaint
            {
                Title = viewModel.Title,
                Description = viewModel.Description,
                Category = viewModel.Category,
                SubmittedByUserId = user.Id,
                AssignedToUserId = viewModel.RecipientUserId,
                CreatedAt = DateTime.UtcNow,
                Status = ComplaintStatus.New
            };

            if (viewModel.Attachment != null && viewModel.Attachment.Length > 0)
            {
                var attachment = await SaveAttachmentAsync(viewModel.Attachment);
                if (attachment != null)
                {
                    complaint.Attachments = new List<ComplaintAttachment> { attachment };
                }
            }

            _context.Complaints.Add(complaint);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var complaint = await _context.Complaints
                .Include(c => c.SubmittedBy)
                .Include(c => c.AssignedTo)
                .Include(c => c.Attachments)
                .Include(c => c.Comments)
                    .ThenInclude(comment => comment.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (complaint == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);

            if (!CanAccessComplaint(complaint, currentUser))
            {
                return Forbid();
            }

            var viewModel = new ComplaintDetailViewModel
            {
                Complaint = complaint,
                Comments = complaint.Comments
                    .OrderByDescending(c => c.CreatedAt)
                    .ToList(),
                Attachments = complaint.Attachments,
                CanUpdateStatus = CanUpdateStatus(complaint, currentUser),
                StatusOptions = Enum.GetValues(typeof(ComplaintStatus))
                    .Cast<ComplaintStatus>()
                    .Select(s => new SelectListItem { Text = s.ToString(), Value = s.ToString(), Selected = s == complaint.Status })
                    .ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int complaintId, ComplaintDetailViewModel viewModel)
        {
            var complaint = await _context.Complaints
                .Include(c => c.Comments)
                .FirstOrDefaultAsync(c => c.Id == complaintId);

            if (complaint == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (!CanAccessComplaint(complaint, user))
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(viewModel.NewComment))
            {
                TempData["CommentError"] = "Comment cannot be empty.";
                return RedirectToAction(nameof(Details), new { id = complaintId });
            }

            var comment = new Comment
            {
                ComplaintId = complaintId,
                UserId = user.Id,
                Content = viewModel.NewComment.Trim()
            };

            complaint.UpdatedAt = DateTime.UtcNow;
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = complaintId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int complaintId, string newStatus)
        {
            var complaint = await _context.Complaints
                .Include(c => c.AssignedTo)
                .FirstOrDefaultAsync(c => c.Id == complaintId);

            if (complaint == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (!CanUpdateStatus(complaint, currentUser))
            {
                return Forbid();
            }

            if (!Enum.TryParse(newStatus, out ComplaintStatus status))
            {
                TempData["StatusError"] = "Invalid status selected.";
                return RedirectToAction(nameof(Details), new { id = complaintId });
            }

            complaint.Status = status;
            complaint.UpdatedAt = DateTime.UtcNow;

            if (status != ComplaintStatus.New)
            {
                complaint.IsEscalated = false;
                complaint.EscalatedAt = null;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = complaintId });
        }

        private bool CanAccessComplaint(Complaint complaint, ApplicationUser user)
        {
            if (User.IsInRole("Admin"))
            {
                return true;
            }

            if (complaint.SubmittedByUserId == user.Id || complaint.AssignedToUserId == user.Id)
            {
                return true;
            }

            if (User.IsInRole("HOD"))
            {
                return complaint.SubmittedBy?.DepartmentId == user.DepartmentId
                    || complaint.AssignedTo?.DepartmentId == user.DepartmentId;
            }

            return false;
        }

        private bool CanUpdateStatus(Complaint complaint, ApplicationUser user)
        {
            if (User.IsInRole("Admin"))
            {
                return true;
            }

            if (User.IsInRole("HOD"))
            {
                return complaint.SubmittedBy?.DepartmentId == user.DepartmentId
                    || complaint.AssignedTo?.DepartmentId == user.DepartmentId;
            }

            return User.IsInRole("Teacher") && complaint.AssignedToUserId == user.Id;
        }

        private IEnumerable<SelectListItem> GetCategoryOptions()
        {
            var categories = new[] { "Academic", "Infrastructure", "Hostel", "Administrative", "Other" };
            return categories.Select(category => new SelectListItem { Text = category, Value = category });
        }

        private async Task<IEnumerable<SelectListItem>> GetRecipientOptionsAsync(ApplicationUser currentUser)
        {
            var options = new List<SelectListItem>();
            var teacherGroup = new SelectListGroup { Name = "Teachers" };
            var hodGroup = new SelectListGroup { Name = "Heads of Department" };
            var adminGroup = new SelectListGroup { Name = "Administrators" };

            var departmentId = currentUser.DepartmentId;

            if (User.IsInRole("Student"))
            {
                var teachers = await _userManager.GetUsersInRoleAsync("Teacher");
                if (departmentId.HasValue)
                {
                    options.AddRange(teachers
                        .Where(t => t.DepartmentId == departmentId)
                        .OrderBy(t => t.FirstName)
                        .Select(t => new SelectListItem
                        {
                            Text = $"{t.FirstName} {t.LastName}",
                            Value = t.Id,
                            Group = teacherGroup
                        }));
                }

                var hods = await _userManager.GetUsersInRoleAsync("HOD");
                var hodMatches = hods
                    .Where(h => !departmentId.HasValue || h.DepartmentId == departmentId)
                    .OrderBy(h => h.FirstName)
                    .Select(h => new SelectListItem
                    {
                        Text = $"{h.FirstName} {h.LastName}",
                        Value = h.Id,
                        Group = hodGroup
                    });
                options.AddRange(hodMatches);
            }
            else if (User.IsInRole("Teacher"))
            {
                var hods = await _userManager.GetUsersInRoleAsync("HOD");
                options.AddRange(hods
                    .Where(h => !departmentId.HasValue || h.DepartmentId == departmentId)
                    .OrderBy(h => h.FirstName)
                    .Select(h => new SelectListItem
                    {
                        Text = $"{h.FirstName} {h.LastName}",
                        Value = h.Id,
                        Group = hodGroup
                    }));
            }
            else if (User.IsInRole("HOD"))
            {
                var admins = await _userManager.GetUsersInRoleAsync("Admin");
                options.AddRange(admins
                    .OrderBy(a => a.FirstName)
                    .Select(a => new SelectListItem
                    {
                        Text = $"{a.FirstName} {a.LastName}",
                        Value = a.Id,
                        Group = adminGroup
                    }));
            }

            return options
                .GroupBy(o => o.Value)
                .Select(g => g.First())
                .OrderBy(o => o.Group?.Name)
                .ThenBy(o => o.Text)
                .ToList();
        }

        private async Task<ComplaintAttachment> SaveAttachmentAsync(IFormFile file)
        {
            var uploadsRoot = Path.Combine(_environment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsRoot);

            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var fullPath = Path.Combine(uploadsRoot, fileName);

            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            _logger.LogInformation("Attachment saved to {Path}", fullPath);

            return new ComplaintAttachment
            {
                FilePath = $"/uploads/{fileName}"
            };
        }
    }
}
