using Microsoft.AspNetCore.Http;

namespace OrchardCore.Infrastructure;

public static class HttpContextExtensions
{
    public static void SignalReleaseShellContext(this HttpContext context)
    {
        if (context.Items.TryGetValue(nameof(ShellSettingsReleaseRequest), out var request) &&
            request is ShellSettingsReleaseRequest shellSettingsReleaseRequest)
        {
            shellSettingsReleaseRequest.Release = false;
        }
        else
        {
            shellSettingsReleaseRequest = new ShellSettingsReleaseRequest()
            {
                Release = true
            };
        }

        context.Items[nameof(ShellSettingsReleaseRequest)] = shellSettingsReleaseRequest;

    }

    public static void ConcealReleaseShellContext(this HttpContext context)
    {
        if (context.Items.TryGetValue(nameof(ShellSettingsReleaseRequest), out var request) &&
            request is ShellSettingsReleaseRequest shellSettingsReleaseRequest)
        {
            shellSettingsReleaseRequest.Release = false;

            context.Items[nameof(ShellSettingsReleaseRequest)] = shellSettingsReleaseRequest;
        }
    }
}
