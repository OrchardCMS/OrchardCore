using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace OrchardCore.ResourceManagement;

public class ResourceRequiredContext
{
    private const string NotIE = "!IE";

    public ResourceDefinition Resource { get; set; }
    public RequireSettings Settings { get; set; }
    public IFileVersionProvider FileVersionProvider { get; set; }

    public void WriteTo(TextWriter writer, string appPath)
    {
        if (string.IsNullOrEmpty(Settings.Condition))
        {
            Resource.WriteTo(writer, Settings, appPath, FileVersionProvider);
            return;
        }

        if (Settings.Condition == NotIE)
        {
            writer.Write("<!--[if " + Settings.Condition + "]>-->");
        }
        else
        {
            writer.Write("<!--[if " + Settings.Condition + "]>");
        }

        Resource.WriteTo(writer, Settings, appPath, FileVersionProvider);

        if (Settings.Condition == NotIE)
        {
            writer.Write("<!--<![endif]-->");
        }
        else
        {
            writer.Write("<![endif]-->");
        }
    }
}
