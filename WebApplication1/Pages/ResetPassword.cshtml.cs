using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using BCrypt.Net;

namespace WebApplication1.Pages
{
    public class ResetPasswordModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ResetPasswordModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public string Token { get; set; } = string.Empty;

        [BindProperty]
        public string NewPassword { get; set; } = string.Empty;

        [BindProperty]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
        public bool IsValidToken { get; set; } = true;

        public async Task<IActionResult> OnGetAsync()
        {
            if (string.IsNullOrEmpty(Token))
            {
                Message = "Invalid reset link.";
                IsSuccess = false;
                IsValidToken = false;
                return Page();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.ResetToken == Token);

            if (user == null || user.ResetTokenExpiry < DateTime.Now)
            {
                Message = "This reset link has expired or is invalid.";
                IsSuccess = false;
                IsValidToken = false;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (NewPassword != ConfirmPassword)
            {
                Message = "Passwords do not match!";
                IsSuccess = false;
                return Page();
            }

            if (NewPassword.Length < 6)
            {
                Message = "Password must be at least 6 characters!";
                IsSuccess = false;
                return Page();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.ResetToken == Token);

            if (user == null || user.ResetTokenExpiry < DateTime.Now)
            {
                Message = "This reset link has expired or is invalid.";
                IsSuccess = false;
                IsValidToken = false;
                return Page();
            }

            // Update password
            user.Password = BCrypt.Net.BCrypt.HashPassword(NewPassword);
            user.ResetToken = null;
            user.ResetTokenExpiry = null;

            await _context.SaveChangesAsync();

            Message = "Password reset successful! Redirecting to login...";
            IsSuccess = true;

            // Redirect after 2 seconds
            Response.Headers.Add("Refresh", "2; url=/Login");

            return Page();
        }
    }
}
