using Microsoft.AspNetCore.Http;
using WebApplication1.Models;

namespace WebApplication1.Helpers;

public static class SessionUserHelper
{
    public const string UserIdKey = "UserId";
    public const string UsernameKey = "Username";
    public const string EmailKey = "Email";
    public const string RoleKey = "Role";

    public static void SetUserSession(ISession session, User user)
    {
        session.SetInt32(UserIdKey, user.Id);
        session.SetString(UsernameKey, user.Username);
        session.SetString(EmailKey, user.Email);
        session.SetString(RoleKey, user.Role);
    }

    public static bool IsAuthenticated(ISession session)
    {
        return session.GetInt32(UserIdKey).HasValue;
    }

    public static (int? UserId, string Username, string Email, string Role) GetUserContext(ISession session)
    {
        return (
            session.GetInt32(UserIdKey),
            session.GetString(UsernameKey) ?? "Guest",
            session.GetString(EmailKey) ?? "N/A",
            session.GetString(RoleKey) ?? "N/A");
    }

    public static void Logout(ISession session)
    {
        session.Clear();
    }
}
