using Microsoft.Playwright;

namespace OrchardCore.Tests.Functional.Helpers;

public static class ConsoleHelper
{
    // Subscribes immediately so errors thrown by scripts injected before the caller
    // gets a chance to assert (e.g. during the initial page load itself) aren't missed.
    public static List<string> CollectConsoleErrors(this IPage page)
    {
        var errors = new List<string>();

        page.Console += (_, message) =>
        {
            if (message.Type == "error")
            {
                errors.Add(message.Text);
            }
        };

        return errors;
    }
}
