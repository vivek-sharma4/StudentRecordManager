using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentRecordManager.Models;
using StudentRecordManager.Repositories.Interfaces;
using StudentRecordManager.Services.Interfaces;
using StudentRecordManager.ViewModels;
using System.Security.Claims;

namespace StudentRecordManager.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ILogger<AccountController> _logger;
        private readonly IDepartmentService _departmentService;

        public AccountController(
            IAccountRepository accountRepository,
            IPasswordHasher<User> passwordHasher,
            IDepartmentService departmentService,
            ILogger<AccountController> logger)
        {
            _accountRepository = accountRepository;
            _passwordHasher = passwordHasher;
            _departmentService = departmentService;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Student");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginViewModel model,
            string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ReturnUrl"] = returnUrl;
                return View(model);
            }

            try
            {
                var trimmedEmail = model.Email.Trim().ToLowerInvariant();
                var user = await _accountRepository.GetUserByEmailAsync(trimmedEmail);

                if (user == null)
                {
                    _logger.LogWarning("Failed login attempt for unknown user {Email}", model.Email);

                    ModelState.AddModelError(string.Empty, "Invalid email address or password.");
                    return View(model);
                }

                var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
                if (verificationResult == PasswordVerificationResult.Failed)
                {
                    ModelState.AddModelError(string.Empty, "Invalid email address or password.");

                    _logger.LogWarning("Failed login attempt for user {Email}", model.Email);
                    return View(model);
                }

                if (!user.IsAuthorised)
                {
                    _logger.LogWarning("Unauthorized user {Email} blocked from login pending admin approval.", user.Email);
                    TempData["PendingEmail"] = user.Email;
                    return RedirectToAction(nameof(PendingApproval));
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("IsAuthorised", user.IsAuthorised.ToString())
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var principal = new ClaimsPrincipal(identity);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    AllowRefresh = true,
                    ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(14) : DateTimeOffset.UtcNow.AddHours(2)
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

                _logger.LogInformation("User {Email} logged in successfully.", user.Email);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Student");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user login.");
                ModelState.AddModelError(string.Empty, "Unable to process login. Please try again.");
                return View(model);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Student");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            RegisterViewModel model,
            string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ReturnUrl"] = returnUrl;
                return View(model);
            }

            try
            {
                var trimmedEmail = model.Email.Trim().ToLowerInvariant();

                var existingUser = await _accountRepository.GetUserByEmailAsync(trimmedEmail);
                if (existingUser != null)
                {
                    _logger.LogWarning("Registration failed: Email {Email} already exists.", trimmedEmail);
                    ModelState.AddModelError("Email", "An account with this email address already exists.");
                    return View(model);
                }

                var newUser = new User
                {
                    Email = trimmedEmail,
                    IsAuthorised = false,
                    Role = "User",
                    CreatedAt = DateTime.UtcNow,
                    Department = string.Empty,
                };

                newUser.PasswordHash = _passwordHasher.HashPassword(newUser, model.Password);

                var newUserId = await _accountRepository.CreateUserAsync(newUser);
                newUser.UserID = newUserId;

                _logger.LogInformation("New user registered with UserID {UserID} and Email {Email}. Awaiting Admin approval.", newUserId, trimmedEmail);

                TempData["PendingEmail"] = trimmedEmail;
                return RedirectToAction(nameof(PendingApproval));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration for {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "Unable to complete registration. Please try again.");
                return View(model);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult PendingApproval()
        {
            ViewBag.PendingEmail = TempData["PendingEmail"] as string;
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> UserApprovals()
        {
            try
            {
                var pendingUsers = await _accountRepository.GetPendingUsersAsync();
                var allUsers = await _accountRepository.GetAllUsersAsync();
                var departments = await _departmentService.GetAllActiveDepartmentsAsync();

                ViewBag.AllUsers = allUsers;
                ViewBag.Departments = departments;

                return View(pendingUsers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending registration requests.");
                TempData["ErrorMessage"] = "Unable to retrieve registration requests.";

                ViewBag.Departments = Enumerable.Empty<Department>();

                return View(new List<User>());
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveUser(int userId, int departmentId)
        {
            if (departmentId <= 0)
            {
                TempData["ErrorMessage"] = "Please select a department before approving the user.";
                return RedirectToAction(nameof(UserApprovals));
            }

            try
            {
                var departments = await _departmentService.GetAllActiveDepartmentsAsync();

                var department = departments.FirstOrDefault(d => d.DepartmentID == departmentId);

                if (department == null)
                {
                    TempData["ErrorMessage"] = "The selected department is invalid or inactive.";
                    return RedirectToAction(nameof(UserApprovals));
                }

                var success = await _accountRepository.ApproveUserAsync(userId, department.DepartmentName);

                if (success)
                {
                    _logger.LogInformation(
                        "Admin {Admin} approved UserID {UserID} and assigned Department {Department}.",
                        User.Identity?.Name,
                        userId,
                        department.DepartmentName);

                    TempData["SuccessMessage"] = $"User has been approved and assigned to the {department.DepartmentName} department.";
                }
                else
                {
                    TempData["ErrorMessage"] = "User not found or approval failed.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving UserID {UserID}.", userId);
                TempData["ErrorMessage"] = "An error occurred while approving the user.";
            }

            return RedirectToAction(nameof(UserApprovals));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectUser(int userId)
        {
            try
            {
                var success = await _accountRepository.DeleteUserAsync(userId);
                if (success)
                {
                    _logger.LogInformation("Admin {Admin} rejected and deleted registration for UserID {UserID}.", User.Identity?.Name, userId);
                    TempData["SuccessMessage"] = "User registration was rejected and removed.";
                }
                else
                {
                    TempData["ErrorMessage"] = "User not found or deletion failed.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting UserID {UserID}.", userId);
                TempData["ErrorMessage"] = "An error occurred while rejecting the user.";
            }

            return RedirectToAction(nameof(UserApprovals));
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            _logger.LogInformation("User {Username} logged out.", User.Identity?.Name);
            return RedirectToAction(nameof(Login));
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}