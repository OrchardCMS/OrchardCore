namespace OrchardCore.Email;

public static class SmtpSettingsExtensions
{
    public static bool HasValidSettings(this SmtpSettings model)
    {
        if (string.IsNullOrEmpty(model.DefaultSender))
        {
            return false;
        }

        return model.DeliveryMethod == SmtpDeliveryMethod.SpecifiedPickupDirectory
            || (model.DeliveryMethod == SmtpDeliveryMethod.Network && !string.IsNullOrEmpty(model.Host));
    }
}
