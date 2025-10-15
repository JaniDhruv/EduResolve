using System;
using System.Linq;
using System.Threading.Tasks;
using EduResolve.Data;
using EduResolve.Models;
using EduResolve.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EduResolve.Controllers
{
    [Authorize(Roles = "HOD,Admin")]
    public class HODController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HODController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> AllComplaints(string status = null, string teacherId = null, string studentId = null)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var query = _context.Complaints
                .Include(c => c.SubmittedBy)
                .Include(c => c.AssignedTo)
                .AsQueryable();

            if (!User.IsInRole("Admin") && currentUser.DepartmentId.HasValue)
            {
                var departmentId = currentUser.DepartmentId.Value;
                query = query.Where(c =>
                    (c.SubmittedBy != null && c.SubmittedBy.DepartmentId == departmentId) ||
                    (c.AssignedTo != null && c.AssignedTo.DepartmentId == departmentId));
            }

            if (!string.IsNullOrEmpty(status) && Enum.TryParse(status, out ComplaintStatus parsedStatus))
            {
                query = query.Where(c => c.Status == parsedStatus);
            }

            if (!string.IsNullOrEmpty(teacherId))
            {
                query = query.Where(c => c.AssignedToUserId == teacherId);
            }

            if (!string.IsNullOrEmpty(studentId))
            {
                query = query.Where(c => c.SubmittedByUserId == studentId);
            }

            var complaints = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();

            var teachers = await _userManager.GetUsersInRoleAsync("Teacher");
            var students = await _userManager.GetUsersInRoleAsync("Student");

            if (!User.IsInRole("Admin") && currentUser.DepartmentId.HasValue)
            {
                var deptId = currentUser.DepartmentId.Value;
                teachers = teachers.Where(t => t.DepartmentId == deptId).ToList();
                students = students.Where(s => s.DepartmentId == deptId).ToList();
            }

            var model = new HodComplaintListViewModel
            {
                Complaints = complaints,
                SelectedStatus = status,
                SelectedTeacherId = teacherId,
                SelectedStudentId = studentId,
                StatusOptions = Enum.GetNames(typeof(ComplaintStatus))
                    .Select(s => new SelectListItem { Text = s, Value = s, Selected = s == status })
                    .ToList(),
                TeacherOptions = teachers
                    .Select(t => new SelectListItem { Text = $"{t.FirstName} {t.LastName}", Value = t.Id, Selected = t.Id == teacherId })
                    .ToList(),
                StudentOptions = students
                    .Select(su => new SelectListItem { Text = $"{su.FirstName} {su.LastName}", Value = su.Id, Selected = su.Id == studentId })
                    .ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int complaintId, string newStatus)
        {
            var complaint = await _context.Complaints.FindAsync(complaintId);
            if (complaint == null)
            {
                return NotFound();
            }

            if (!Enum.TryParse(newStatus, out ComplaintStatus status))
            {
                TempData["StatusError"] = "Invalid status provided.";
                return RedirectToAction(nameof(AllComplaints));
            }

            complaint.Status = status;
            complaint.UpdatedAt = DateTime.UtcNow;

            if (status != ComplaintStatus.New)
            {
                complaint.IsEscalated = false;
                complaint.EscalatedAt = null;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(AllComplaints), new { status });
        }

        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> Reassign(int complaintId, string newAssigneeId)
        // {
        //     var complaint = await _context.Complaints.FindAsync(complaintId);
        //     if (complaint == null)
        //     {
        //         return NotFound();
        //     }

        //     if (string.IsNullOrEmpty(newAssigneeId))
        //     {
        //         TempData["ReassignError"] = "Please select a valid assignee.";
        //         return RedirectToAction(nameof(AllComplaints));
        //     }

        //     var assignee = await _userManager.FindByIdAsync(newAssigneeId);
        //     if (assignee == null || !await _userManager.IsInRoleAsync(assignee, "Teacher"))
        //     {
        //         TempData["ReassignError"] = "Assignee must be a teacher.";
        //         return RedirectToAction(nameof(AllComplaints));
        //     }

        //     complaint.AssignedToUserId = assignee.Id;
        //     complaint.IsEscalated = false;
        //     complaint.Status = ComplaintStatus.InProgress;
        //     complaint.UpdatedAt = DateTime.UtcNow;

        //     await _context.SaveChangesAsync();
        //     return RedirectToAction(nameof(AllComplaints));
        // }
    }
}
