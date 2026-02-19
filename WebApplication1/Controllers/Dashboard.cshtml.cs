using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApplication1.Pages
{
    public class DashboardModel : PageModel
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public int? UserId { get; set; }

        public IActionResult OnGet()
        {
            // Check if user is logged in
            UserId = HttpContext.Session.GetInt32("UserId");

            if (UserId == null)
            {
                // Not logged in, redirect to login page
                return RedirectToPage("/Login");
            }

            // Get user info from session
            Username = HttpContext.Session.GetString("Username") ?? "Guest";
            Email = HttpContext.Session.GetString("Email") ?? "N/A";
            Role = HttpContext.Session.GetString("Role") ?? "N/A";

            return Page();
        }

        public IActionResult OnPostLogout()
        {
            // Clear all session data
            HttpContext.Session.Clear();

            // Redirect back to login page
            return RedirectToPage("/Login");
        }
    }
}
