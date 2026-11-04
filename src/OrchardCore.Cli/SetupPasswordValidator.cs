namespace OrchardCore.Cli;

internal static class SetupPasswordValidator
{
    // ASP.NET Core Identity's default setup policy. Host-specific policies are
    // also checked by the server; never echo the supplied secret in an error.
    public static void Validate(string? password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < 6
            || !password.Any(char.IsAsciiLetterUpper)
            || !password.Any(char.IsAsciiLetterLower)
            || !password.Any(char.IsAsciiDigit)
            || password.All(char.IsAsciiLetterOrDigit))
        {
            throw new CliException("The setup administrator password must contain at least 6 characters, including an uppercase letter (A-Z), a lowercase letter (a-z), a digit (0-9), and a non-alphanumeric character. No tenant creation or setup request was sent.");
        }
    }
}
