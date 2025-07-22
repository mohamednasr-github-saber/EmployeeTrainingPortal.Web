using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmployeeTrainingPortal.Models;
using EmployeeTrainingPortal.ViewModels;

namespace EmployeeTrainingPortal.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var totalCourses = await _context.Courses.CountAsync();
            var totalInstructors = await _context.Users
                .CountAsync(u => _context.UserRoles
                    .Any(ur => ur.UserId == u.Id && _context.Roles
                        .Any(r => r.Id == ur.RoleId && r.Name == "Instructor")));

            var totalEmployees = await _context.Users
                .CountAsync(u => _context.UserRoles
                    .Any(ur => ur.UserId == u.Id && _context.Roles
                        .Any(r => r.Id == ur.RoleId && r.Name == "Employee")));

            var totalEnrollments = await _context.Enrollments.CountAsync();
            var totalAssessments = await _context.Assessments.CountAsync();
            var averageFeedbackRating = await _context.Feedbacks.AnyAsync()
                ? await _context.Feedbacks.AverageAsync(f => f.Rating)
                : 0;

            var viewModel = new AdminDashboardViewModel
            {
                TotalCourses = totalCourses,
                TotalInstructors = totalInstructors,
                TotalEmployees = totalEmployees,
                TotalEnrollments = totalEnrollments,
                TotalAssessments = totalAssessments,
                AverageFeedbackRating = averageFeedbackRating
            };

            return View(viewModel);
        }
    }
}
