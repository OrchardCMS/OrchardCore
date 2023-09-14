namespace OrchardCore.Users;

public class IdentityErrorCode
{
    public const string PasswordRequiresDigit = nameof(PasswordRequiresDigit);

    public const string PasswordRequiresLower = nameof(PasswordRequiresLower);

    public const string PasswordRequiresUpper = nameof(PasswordRequiresUpper);

    public const string PasswordRequiresNonAlphanumeric = nameof(PasswordRequiresNonAlphanumeric);

    public const string PasswordTooShort = nameof(PasswordTooShort);

    public const string PasswordRequiresUniqueChars = nameof(PasswordRequiresUniqueChars);

    public const string PasswordMismatch = nameof(PasswordMismatch);

    public const string InvalidUserName = nameof(InvalidUserName);

    public const string DuplicateUserName = nameof(DuplicateUserName);

    public const string DuplicateEmail = nameof(DuplicateEmail);

    public const string InvalidEmail = nameof(InvalidEmail);
}
