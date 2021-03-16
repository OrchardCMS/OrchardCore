namespace OrchardCore.Environment.Commands
{
    /// <summary>
    /// Different return codes for a command execution.
    /// </summary>
    public enum CommandReturnCodes
    {
        Ok = 0,
        Fail = 5,
        Retry = 240
    }
}
