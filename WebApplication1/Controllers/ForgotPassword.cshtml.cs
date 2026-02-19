using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Services;

namespace WebApplication1.Pages
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public ForgotPasswordModel(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string VerificationCode { get; set; } = string.Empty;

        [BindProperty]
        public string NewPassword { get; set; } = string.Empty;

        [BindProperty]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
        public bool CodeSent { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostSendCodeAsync()
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == Email);

            if (user == null)
            {
                Message = "If this email exists, a verification code has been sent.";
                IsSuccess = true;
                CodeSent = true;
                return Page();
            }

            // Generate 6-digit code
            var random = new Random();
            var code = random.Next(100000, 999999).ToString();

            user.VerificationCode = code;
            user.VerificationCodeExpiry = DateTime.Now.AddMinutes(15);

            await _context.SaveChangesAsync();

            try
            {
                await _emailService.SendVerificationCodeAsync(Email, code);
                Message = "Verification code has been sent to your email!";
                IsSuccess = true;
                CodeSent = true;
            }
            catch (Exception ex)
            {
                Message = $"Error sending email: {ex.Message}";
                IsSuccess = false;
                CodeSent = false;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostVerifyCodeAsync()
        {
            CodeSent = true;

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
                .FirstOrDefaultAsync(u => u.Email == Email);

            if (user == null || user.VerificationCode != VerificationCode)
            {
                Message = "Invalid verification code!";
                IsSuccess = false;
                return Page();
            }

            if (user.VerificationCodeExpiry < DateTime.Now)
            {
                Message = "Verification code has expired! Please request a new one.";
                IsSuccess = false;
                return Page();
            }

            // Update password
            user.Password = BCrypt.Net.BCrypt.HashPassword(NewPassword);
            user.VerificationCode = null;
            user.VerificationCodeExpiry = null;
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
