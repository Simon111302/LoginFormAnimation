using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Helpers;

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
            if (!SessionUserHelper.IsAuthenticated(HttpContext.Session))
            {
                return RedirectToPage("/Login");
            }

            var user = SessionUserHelper.GetUserContext(HttpContext.Session);
            UserId = user.UserId;
            Username = user.Username;
            Email = user.Email;
            Role = user.Role;

            return Page();
        }

        public IActionResult OnPostLogout()
        {
            SessionUserHelper.Logout(HttpContext.Session);
            return RedirectToPage("/Login");
        }
    }
}
