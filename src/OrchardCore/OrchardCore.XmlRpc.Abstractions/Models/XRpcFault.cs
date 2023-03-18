namespace OrchardCore.XmlRpc.Models;

public class XRpcFault
{
    public XRpcFault(int code, string message)
    {
        Code = code;
        Message = message;
    }

    public string Message { get; }

    public int Code { get; }
}
