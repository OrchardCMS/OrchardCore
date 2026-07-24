using OrchardCore.DataOrchestrator.Activities;
using OrchardCore.DataOrchestrator.Display;
using OrchardCore.DataOrchestrator.Exporting;
using OrchardCore.DataOrchestrator.ViewModels;

namespace OrchardCore.DataOrchestrator.Drivers;

public sealed class FtpExportLoadDisplayDriver : EtlActivityDisplayDriver<FtpExportLoad, FtpExportLoadViewModel>
{
    private readonly IEtlExportFormatProvider _formatProvider;

    public FtpExportLoadDisplayDriver(IEtlExportFormatProvider formatProvider)
    {
        _formatProvider = formatProvider;
    }

    protected override void EditActivity(FtpExportLoad activity, FtpExportLoadViewModel model)
    {
        model.Format = activity.Format;
        model.FileName = activity.FileName;
        model.Formats = FileExportLoadDriverHelper.BuildFormatOptions(_formatProvider);
        model.Host = activity.Host;
        model.Port = activity.Port;
        model.Username = activity.Username;
        model.Password = null;
        model.RemoteDirectory = activity.RemoteDirectory;
        model.SecurityMode = activity.SecurityMode;
        model.AcceptAnyCertificate = activity.AcceptAnyCertificate;
        model.Passive = activity.Passive;
    }

    protected override void UpdateActivity(FtpExportLoadViewModel model, FtpExportLoad activity)
    {
        activity.Format = model.Format;
        activity.FileName = model.FileName;
        activity.Host = model.Host;
        activity.Port = model.Port;
        activity.Username = model.Username;

        if (!string.IsNullOrEmpty(model.Password))
        {
            activity.Password = model.Password;
        }

        activity.RemoteDirectory = model.RemoteDirectory;
        activity.SecurityMode = model.SecurityMode;
        activity.AcceptAnyCertificate = model.AcceptAnyCertificate;
        activity.Passive = model.Passive;
    }
}
