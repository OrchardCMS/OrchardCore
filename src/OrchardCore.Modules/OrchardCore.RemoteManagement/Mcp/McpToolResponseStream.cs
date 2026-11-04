namespace OrchardCore.RemoteManagement.Mcp;

/// <summary>
/// Buffers the response of the endpoint called by an MCP tool, which becomes the text of its result, up to a limit.
/// </summary>
internal sealed class McpToolResponseStream : MemoryStream
{
    private readonly int _maximumLength;

    public McpToolResponseStream(int maximumLength)
    {
        _maximumLength = maximumLength;
    }

    /// <summary>
    /// Gets whether the endpoint tried to write more than the limit, even if it then handled the exception.
    /// </summary>
    public bool LimitExceeded { get; private set; }

    public override void Write(byte[] buffer, int offset, int count)
    {
        EnsureCapacityLeft(count);
        base.Write(buffer, offset, count);
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        EnsureCapacityLeft(buffer.Length);
        base.Write(buffer);
    }

    public override void WriteByte(byte value)
    {
        EnsureCapacityLeft(1);
        base.WriteByte(value);
    }

    private void EnsureCapacityLeft(int count)
    {
        if (Length + count > _maximumLength)
        {
            LimitExceeded = true;

            throw new McpToolResponseTooLargeException();
        }
    }
}

/// <summary>
/// Stops the endpoint called by an MCP tool when its response is larger than the limit.
/// </summary>
internal sealed class McpToolResponseTooLargeException : Exception
{
}
