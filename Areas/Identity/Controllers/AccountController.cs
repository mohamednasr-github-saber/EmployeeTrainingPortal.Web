using EmployeeTrainingPortal.Models.ViewModels;
using EmployeeTrainingPortal.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity.UI.Services;
using EmployeeTrainingPortal.DAL.Repositories.IRepositories;

namespace EmployeeTrainingPortal.Web.Areas.Identity.Controllers
{
    [Area("Identity")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSender _emailSender;
        private readonly IApplicationUserOtpRepository _otpRepository;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IEmailSender emailSender,
            IApplicationUserOtpRepository otpRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
            _otpRepository = otpRepository;
        }

        // GET: Register
        public async Task<IActionResult> Register()
        {
            if (!_roleManager.Roles.Any())
            {
                await _roleManager.CreateAsync(new IdentityRole(SD.Admin));
                await _roleManager.CreateAsync(new IdentityRole(SD.Instructor));
                await _roleManager.CreateAsync(new IdentityRole(SD.Employee));
            }

            return View();
        }

        // POST: Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (!ModelState.IsValid)
                return View(registerVM);

            var user = new ApplicationUser
            {
                UserName = registerVM.Email,
                Email = registerVM.Email,
                NormalizedUserName = registerVM.UserName
            };

            var result = await _userManager.CreateAsync(user, registerVM.Password);

            if (result.Succeeded)
            {
                string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = Url.Action("ConfirmEmail", "Account", new { area = "Identity", Id = user.Id, token }, Request.Scheme);

                await _emailSender.SendEmailAsync(user.Email, "Confirm your email", $"Click <a href='{confirmationLink}'>here</a> to confirm your email.");

                await _userManager.AddToRoleAsync(user, SD.Employee);
                //await _userManager.AddToRoleAsync(user, SD.Instructor);


                TempData["Notification"] = "Account created successfully. Please confirm your email.";
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(registerVM);
        }

        // GET: Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if (!ModelState.IsValid)
                return View(loginVM);

            var user = await _userManager.FindByEmailAsync(loginVM.UserNameOREmail)
                        ?? await _userManager.FindByNameAsync(loginVM.UserNameOREmail);

            if (user == null)
            {
                ModelState.AddModelError("Email", "Invalid Email or Password");
                return View(loginVM);
            }

            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                ModelState.AddModelError("Email", "Please confirm your email before logging in.");
                return View(loginVM);
            }

            var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, loginVM.RememberMe, false);

            if (result.Succeeded)
            {
                TempData["Notification"] = "Login successful";

                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains(SD.Admin))
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });

                if (roles.Contains(SD.Instructor))
                    return RedirectToAction("Index", "Dashboard", new { area = "Instructor" });

                if (roles.Contains(SD.Employee))
                    return RedirectToAction("Index", "Enrollment", new { area = "Enrollment" });

                return RedirectToAction("Login");
            }

            ModelState.AddModelError("Password", "Invalid Email or Password");
            return View(loginVM);
        }

        // Logout
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["Notification"] = "Logged out successfully";
            return RedirectToAction("Login");
        }

        public IActionResult ResendEmailConfirmation()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResendEmailConfirmation(ResendEmailConfirmationVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.UserNameOREmail)
                        ?? await _userManager.FindByNameAsync(model.UserNameOREmail);

            if (user == null)
            {
                ModelState.AddModelError("UserNameOREmail", "Invalid Username or Email");
                return View(model);
            }

            if (user.EmailConfirmed)
            {
                TempData["Notification-error"] = "Email is already confirmed.";
                return RedirectToAction("Login");
            }

            string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = Url.Action("ConfirmEmail", "Account", new { area = "Identity", Id = user.Id, token }, Request.Scheme);

            await _emailSender.SendEmailAsync(user.Email, "Resend Confirmation", $"Click <a href='{confirmationLink}'>here</a> to confirm your email.");

            TempData["Notification"] = "Confirmation email sent.";
            return RedirectToAction("Login");
        }

        public async Task<IActionResult> ConfirmEmail(string Id, string token)
        {
            var user = await _userManager.FindByIdAsync(Id);

            if (user == null)
                return BadRequest();

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                TempData["Notification"] = "Email confirmed successfully.";
                return RedirectToAction("Login");
            }

            TempData["Notification-error"] = string.Join(", ", result.Errors.Select(e => e.Description));
            return RedirectToAction("Login");
        }

        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.UserNameOREmail)
                        ?? await _userManager.FindByNameAsync(model.UserNameOREmail);

            if (user == null)
            {
                ModelState.AddModelError("UserNameOREmail", "Invalid Username or Email");
                return View(model);
            }

            string token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var otp = new Random().Next(1000, 9999);

            await _otpRepository.CreateAsync(new()
            {
                ApplicationUserId = user.Id,
                OTP = otp,
                ReleaseData = DateTime.UtcNow,
                ExpirationData = DateTime.UtcNow.AddMinutes(2)
            });
            await _otpRepository.CommitAsync();

            await _emailSender.SendEmailAsync(user.Email, "Reset Password OTP", $"Use this OTP to reset your password: {otp}");

            TempData["Notification"] = "OTP sent to email.";
            TempData["_ValidationToken"] = Guid.NewGuid().ToString();

            return RedirectToAction("ResetPassword", new { ApplicationUserId = user.Id, Token = token });
        }

        public IActionResult ResetPassword()
        {
            if (TempData["_ValidationToken"] != null)
                return View();

            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByIdAsync(model.ApplicationUserId);

            if (user == null)
                return BadRequest();

            var otpEntry = _otpRepository.Get(e => e.ApplicationUserId == model.ApplicationUserId).LastOrDefault();

            if (otpEntry != null && otpEntry.OTP == model.OTP && otpEntry.ExpirationData > DateTime.UtcNow)
            {
                var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
                if (result.Succeeded)
                {
                    TempData["Notification"] = "Password reset successfully.";
                    return RedirectToAction("Login");
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }
            else
            {
                ModelState.AddModelError("OTP", "Invalid or expired OTP");
            }

            return View(model);
        }
    }
}
