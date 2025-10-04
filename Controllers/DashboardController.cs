using System;
using System.Linq;
using System.Threading.Tasks;
using EduResolve.Data;
using EduResolve.Models;
using EduResolve.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduResolve.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var model = new DashboardViewModel
            {
                Role = User.IsInRole("Admin") ? "Admin" :
                       User.IsInRole("HOD") ? "HOD" :
                       User.IsInRole("Teacher") ? "Teacher" : "Student"
            };

            if (User.IsInRole("Student"))
            {
                var complaints = await _context.Complaints
                    .Where(c => c.SubmittedByUserId == user.Id)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                model.StudentOverview = new StudentDashboardViewModel
                {
                    TotalComplaints = complaints.Count,
                    OpenComplaints = complaints.Count(c => c.Status == ComplaintStatus.New || c.Status == ComplaintStatus.InProgress || c.Status == ComplaintStatus.Reopened),
                    ResolvedComplaints = complaints.Count(c => c.Status == ComplaintStatus.Resolved || c.Status == ComplaintStatus.Closed),
                    RecentComplaints = complaints.Take(5).ToList()
                };
            }
            else if (User.IsInRole("Teacher"))
            {
                var assigned = await _context.Complaints
                    .Where(c => c.AssignedToUserId == user.Id)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                model.TeacherOverview = new TeacherDashboardViewModel
                {
                    NewComplaints = assigned.Count(c => c.Status == ComplaintStatus.New),
                    InProgressComplaints = assigned.Count(c => c.Status == ComplaintStatus.InProgress || c.Status == ComplaintStatus.Reopened),
                    ResolvedByMe = assigned.Count(c => c.Status == ComplaintStatus.Resolved || c.Status == ComplaintStatus.Closed),
                    PendingComplaints = assigned.Where(c => c.Status == ComplaintStatus.New || c.Status == ComplaintStatus.InProgress || c.Status == ComplaintStatus.Reopened)
                        .Take(5)
                        .ToList()
                };
            }
            else if (User.IsInRole("HOD") || User.IsInRole("Admin"))
            {
                var departmentId = user.DepartmentId;
                var departmentalComplaints = await _context.Complaints
                    .Include(c => c.SubmittedBy)
                    .Include(c => c.AssignedTo)
                    .Where(c =>
                        (departmentId.HasValue && c.SubmittedBy != null && c.SubmittedBy.DepartmentId == departmentId)
                        || (departmentId.HasValue && c.AssignedTo != null && c.AssignedTo.DepartmentId == departmentId)
                        || User.IsInRole("Admin"))
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                var resolvedComplaints = departmentalComplaints
                    .Where(c => c.Status == ComplaintStatus.Resolved || c.Status == ComplaintStatus.Closed)
                    .ToList();

                double averageResolutionHours = 0;
                if (resolvedComplaints.Any())
                {
                    averageResolutionHours = resolvedComplaints
                        .Where(c => c.UpdatedAt.HasValue)
                        .Select(c => (c.UpdatedAt.Value - c.CreatedAt).TotalHours)
                        .DefaultIfEmpty(0)
                        .Average();
                }

                model.HodOverview = new HodDashboardViewModel
                {
                    TotalComplaints = departmentalComplaints.Count,
                    ResolvedComplaints = resolvedComplaints.Count,
                    AverageResolutionHours = Math.Round(averageResolutionHours, 2),
                    EscalatedComplaints = departmentalComplaints
                        .Where(c => c.IsEscalated)
                        .OrderByDescending(c => c.EscalatedAt ?? c.CreatedAt)
                        .Take(10)
                        .ToList(),
                    RecentComplaints = departmentalComplaints.Take(10).ToList()
                };
            }

            return View(model);
        }
    }
}
