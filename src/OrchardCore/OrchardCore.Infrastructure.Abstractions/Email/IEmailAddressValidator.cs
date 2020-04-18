namespace OrchardCore.Email
{
    /// <summary>
    /// Contract for e-mail address validation service.
    /// </summary>
    public interface IEmailAddressValidator
    {
        /// <summary>
        /// Validates an e-mail address.
        /// </summary>
        /// <param name="emailAddress">The e-mail address to be validated.</param>
        /// <returns><c>true</c> if the email is valid, otherwise <c>false</c>.</returns>
        bool Validate(string emailAddress);
    }
}
