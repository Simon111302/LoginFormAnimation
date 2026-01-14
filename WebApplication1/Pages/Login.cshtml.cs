using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApplication1.Pages
{
    public class LoginModel : PageModel
    {
        private readonly ILogger<LoginModel> _logger;

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Password { get; set; }

        [BindProperty]
        public string FullName { get; set; }

        public LoginModel(ILogger<LoginModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            // Page load
        }

        public IActionResult OnPostLogin()
        {
            // Validate input
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                ViewData["LoginError"] = "Email and password are required.";
                return Page();
            }

            // TODO: Add your authentication logic here
            // Example: Check against database
            // if (await _userService.ValidateUser(Email, Password))
            // {
            //     // Set authentication cookie
            //     return RedirectToPage("/Index");
            // }

            // For now, just simulate a successful login
            _logger.LogInformation($"User login attempt: {Email}");

            // Redirect to home page after successful login
            return RedirectToPage("/Index");
        }

        public IActionResult OnPostRegister()
        {
            // Validate input
            if (string.IsNullOrEmpty(FullName) || string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                ViewData["RegisterError"] = "All fields are required.";
                return Page();
            }

            // Validate email format
            if (!Email.Contains("@"))
            {
                ViewData["RegisterError"] = "Please enter a valid email address.";
                return Page();
            }

            // Validate password strength
            if (Password.Length < 6)
            {
                ViewData["RegisterError"] = "Password must be at least 6 characters.";
                return Page();
            }

            // TODO: Add your registration logic here
            // Example: Save to database
            // var user = new User { FullName = FullName, Email = Email, Password = HashedPassword };
            // await _userService.CreateUser(user);

            // For now, just log the registration
            _logger.LogInformation($"New user registration: {FullName} ({Email})");

            // Redirect to home page after successful registration
            return RedirectToPage("/Index");
        }
    }
}
