using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Helpers;
using WebApplication1.Models;

namespace WebApplication1.Pages
{
    public class LoginModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LoginModel> _logger;

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        public string FullName { get; set; } = string.Empty;

        public LoginModel(ApplicationDbContext context, ILogger<LoginModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostLoginAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
                {
                    ViewData["LoginError"] = "Email and password are required.";
                    return Page();
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == Email);

                if (user == null)
                {
                    ViewData["LoginError"] = "Invalid email or password.";
                    _logger.LogWarning("Login failed: User not found - {Email}", Email);
                    return Page();
                }

                if (!user.IsActive)
                {
                    ViewData["LoginError"] = "Account is disabled. Please contact administrator.";
                    _logger.LogWarning("Login failed: Account disabled - {Email}", Email);
                    return Page();
                }

                var isPasswordValid = BCrypt.Net.BCrypt.Verify(Password, user.Password);

                if (!isPasswordValid)
                {
                    ViewData["LoginError"] = "Invalid email or password.";
                    _logger.LogWarning("Login failed: Invalid password - {Email}", Email);
                    return Page();
                }

                SessionUserHelper.SetUserSession(HttpContext.Session, user);

                _logger.LogInformation("User logged in successfully: {Email}", Email);
                return RedirectToPage("/Dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                ViewData["LoginError"] = "An error occurred. Please try again.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostRegisterAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(FullName) || string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
                {
                    ViewData["RegisterError"] = "All fields are required.";
                    ViewData["ShowRegisterPanel"] = true;
                    return Page();
                }

                if (!Email.Contains("@") || !Email.Contains("."))
                {
                    ViewData["RegisterError"] = "Please enter a valid email address.";
                    ViewData["ShowRegisterPanel"] = true;
                    return Page();
                }

                if (!PasswordHelper.IsValidLength(Password))
                {
                    ViewData["RegisterError"] = $"Password must be at least {PasswordHelper.MinLength} characters.";
                    ViewData["ShowRegisterPanel"] = true;
                    return Page();
                }

                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == Email || u.Username == FullName);

                if (existingUser != null)
                {
                    ViewData["RegisterError"] = "User with this email or username already exists.";
                    ViewData["ShowRegisterPanel"] = true;
                    return Page();
                }

                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(Password, 12);

                var newUser = new User
                {
                    Username = FullName,
                    Email = Email,
                    Password = hashedPassword,
                    Role = "student",
                    IsActive = true
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                _logger.LogInformation("New user registered: {Email}", Email);

                ViewData["RegisterSuccess"] = $"Account created successfully! You can now sign in with {Email}";
                ViewData["ShowRegisterPanel"] = true;

                return Page();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error during registration");
                ViewData["RegisterError"] = "Database error. Please try again.";
                ViewData["ShowRegisterPanel"] = true;
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                ViewData["RegisterError"] = "An error occurred. Please try again.";
                ViewData["ShowRegisterPanel"] = true;
                return Page();
            }
        }
    }
}
