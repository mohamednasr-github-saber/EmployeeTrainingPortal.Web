using System.ComponentModel.DataAnnotations;

namespace EmployeeTrainingPortal.Web.Areas.Admin.ViewModels
{
    public class CreateUserVM
    {
        [Required]
        public string UserName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        public string Role { get; set; }

        public List<string> Roles { get; set; }
    }
}
