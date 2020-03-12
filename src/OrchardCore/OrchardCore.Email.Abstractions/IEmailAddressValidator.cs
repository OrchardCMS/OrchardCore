namespace OrchardCore.Email
{
    public interface IEmailAddressValidator
    {
        bool Validate(string emailAddress);
    }
}
