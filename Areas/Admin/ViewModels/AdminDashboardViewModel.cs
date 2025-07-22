namespace EmployeeTrainingPortal.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalCourses { get; set; }
        public int TotalUsers { get; set; }
        public int TotalInstructors { get; set; }
        public int TotalEmployees { get; set; }
        public int TotalEnrollments { get; set; }
        public int TotalAssessments { get; set; }
        public double AverageFeedbackRating { get; set; }
        public List<Course> RecentCourses { get; set; } = new(); // Initialize to avoid null reference
    }
}
