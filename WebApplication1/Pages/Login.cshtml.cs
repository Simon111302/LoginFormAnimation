using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using BCrypt.Net;

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

        public void OnGet()
        {
            // Page load
        }

        public async Task<IActionResult> OnPostLoginAsync()
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
                {
                    ViewData["LoginError"] = "Email and password are required.";
                    return Page();
                }

                // Find user by email
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == Email);

                if (user == null)
                {
                    ViewData["LoginError"] = "Invalid email or password.";
                    _logger.LogWarning($"Login failed: User not found - {Email}");
                    return Page();
                }

                // Check if account is active
                if (!user.IsActive)
                {
                    ViewData["LoginError"] = "Account is disabled. Please contact administrator.";
                    _logger.LogWarning($"Login failed: Account disabled - {Email}");
                    return Page();
                }

                // Verify password using BCrypt
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(Password, user.Password);

                if (!isPasswordValid)
                {
                    ViewData["LoginError"] = "Invalid email or password.";
                    _logger.LogWarning($"Login failed: Invalid password - {Email}");
                    return Page();
                }

                // Store user info in session
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("Email", user.Email);
                HttpContext.Session.SetString("Role", user.Role);

                _logger.LogInformation($"User logged in successfully: {Email}");

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
            Console.WriteLine("=== REGISTER METHOD STARTED ===");

            try
            {
                Console.WriteLine($"FullName: {FullName}, Email: {Email}, Password length: {Password?.Length}");

                // Validate input
                if (string.IsNullOrEmpty(FullName) || string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
                {
                    Console.WriteLine("ERROR: Missing fields");
                    ViewData["RegisterError"] = "All fields are required.";
                    ViewData["ShowRegisterPanel"] = true;
                    return Page();
                }

                // Validate email format
                if (!Email.Contains("@") || !Email.Contains("."))
                {
                    Console.WriteLine("ERROR: Invalid email format");
                    ViewData["RegisterError"] = "Please enter a valid email address.";
                    ViewData["ShowRegisterPanel"] = true;
                    return Page();
                }

                // Validate password strength
                if (Password.Length < 6)
                {
                    Console.WriteLine("ERROR: Password too short");
                    ViewData["RegisterError"] = "Password must be at least 8 characters.";
                    ViewData["ShowRegisterPanel"] = true;
                    return Page();
                }

                Console.WriteLine("Checking if user exists...");

                // Check if user already exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == Email || u.Username == FullName);

                if (existingUser != null)
                {
                    Console.WriteLine("ERROR: User already exists");
                    ViewData["RegisterError"] = "User with this email or username already exists.";
                    ViewData["ShowRegisterPanel"] = true;
                    return Page();
                }

                Console.WriteLine("Hashing password...");

                // Hash password using BCrypt
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(Password, 12);

                Console.WriteLine("Creating new user object...");

                // Create new user
                var newUser = new User
                {
                    Username = FullName,
                    Email = Email,
                    Password = hashedPassword,
                    Role = "student",
                    IsActive = true
                };

                Console.WriteLine("Adding user to context...");
                _context.Users.Add(newUser);

                Console.WriteLine("Saving to database...");
                await _context.SaveChangesAsync();

                Console.WriteLine($"SUCCESS! User registered with ID: {newUser.Id}");
                _logger.LogInformation($"New user registered: {Email}");

                // Use ViewData instead of TempData and return Page() to stay on registration panel
                ViewData["RegisterSuccess"] = $"Account created successfully! You can now sign in with {Email}";
                ViewData["ShowRegisterPanel"] = true; // Flag to keep registration panel active

                return Page(); // Stay on the same page, don't redirect
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"DATABASE ERROR: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                _logger.LogError(ex, "Database error during registration");
                ViewData["RegisterError"] = "Database error. Check console output.";
                ViewData["ShowRegisterPanel"] = true;
                return Page();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GENERAL ERROR: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                _logger.LogError(ex, "Error during registration");
                ViewData["RegisterError"] = $"Error: {ex.Message}";
                ViewData["ShowRegisterPanel"] = true;
                return Page();
            }
        }
    }
}
