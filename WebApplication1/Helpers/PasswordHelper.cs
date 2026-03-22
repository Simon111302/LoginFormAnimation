namespace WebApplication1.Helpers;

public static class PasswordHelper
{
    public const int MinLength = 8;

    public static bool IsValidLength(string? password)
    {
        return !string.IsNullOrWhiteSpace(password) && password.Length >= MinLength;
    }
}
