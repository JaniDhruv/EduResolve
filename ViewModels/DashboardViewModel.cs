using System.Collections.Generic;
using EduResolve.Models;

namespace EduResolve.ViewModels
{
    public class DashboardViewModel
    {
        public string Role { get; set; }
        public StudentDashboardViewModel StudentOverview { get; set; }
        public TeacherDashboardViewModel TeacherOverview { get; set; }
        public HodDashboardViewModel HodOverview { get; set; }
    }

    public class StudentDashboardViewModel
    {
        public int TotalComplaints { get; set; }
        public int OpenComplaints { get; set; }
        public int ResolvedComplaints { get; set; }
        public IEnumerable<Complaint> RecentComplaints { get; set; }
    }

    public class TeacherDashboardViewModel
    {
        public int NewComplaints { get; set; }
        public int InProgressComplaints { get; set; }
        public int ResolvedByMe { get; set; }
        public IEnumerable<Complaint> PendingComplaints { get; set; }
    }

    public class HodDashboardViewModel
    {
        public int TotalComplaints { get; set; }
        public int ResolvedComplaints { get; set; }
        public double AverageResolutionHours { get; set; }
        public IEnumerable<Complaint> EscalatedComplaints { get; set; }
        public IEnumerable<Complaint> RecentComplaints { get; set; }
    }
}
