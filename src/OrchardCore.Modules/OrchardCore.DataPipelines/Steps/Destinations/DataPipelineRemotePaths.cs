namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// Builds the paths of the files the upload steps send to a file server, and the text that describes where they went.
/// </summary>
internal static class DataPipelineRemotePaths
{
    /// <summary>
    /// Renders and normalizes a remote folder: placeholders are replaced, separators become <c>/</c>, and the trailing
    /// separator is removed, except for the root folder.
    /// </summary>
    public static string RenderFolder(string template, DataPipelineRunContext run, DateTime now)
    {
        var folder = (DataPipelineTemplate.Render(template?.Trim(), run, now) ?? string.Empty).Replace('\\', '/').Trim();

        if (folder.Length > 1)
        {
            folder = folder.TrimEnd('/');
        }

        return folder;
    }

    /// <summary>
    /// Combines a folder and a file name.
    /// </summary>
    public static string Combine(string folder, string fileName)
        => string.IsNullOrEmpty(folder) ? fileName : folder.EndsWith('/') ? folder + fileName : folder + "/" + fileName;

    /// <summary>
    /// Describes a remote path as a URL, such as <c>sftp://host:22/folder/file.csv</c>. It never holds credentials.
    /// </summary>
    public static string ToDisplayUrl(string scheme, string host, int port, string path)
        => $"{scheme}://{host}:{port}/{(path ?? string.Empty).TrimStart('/')}";

    /// <summary>
    /// Replaces the secrets a message may hold, such as the message of an exception thrown by a client library.
    /// </summary>
    public static string Redact(string message, params string[] secrets)
    {
        if (string.IsNullOrEmpty(message))
        {
            return message;
        }

        foreach (var secret in secrets)
        {
            if (!string.IsNullOrEmpty(secret))
            {
                message = message.Replace(secret, "***", StringComparison.Ordinal);
            }
        }

        return message;
    }

    /// <summary>
    /// Uploads every file of the input of a step to a file server, connecting on the first file.
    /// </summary>
    /// <param name="context">The context of the step.</param>
    /// <param name="createClient">Creates the client.</param>
    /// <param name="connected">Called once the client is connected.</param>
    /// <param name="folder">The rendered remote folder.</param>
    /// <param name="overwrite">Whether to replace existing files.</param>
    /// <param name="describe">Describes the delivery of a file, from its name and remote path.</param>
    /// <param name="exists">Builds the message of the error raised when a file exists and is not overwritten.</param>
    /// <returns>The number of uploaded files.</returns>
    public static async Task<int> UploadAsync(
        DataPipelineStepContext context,
        Func<IDataPipelineFileTransferClient> createClient,
        Action connected,
        string folder,
        bool overwrite,
        Func<string, string, string> describe,
        Func<string, string> exists)
    {
        IDataPipelineFileTransferClient client = null;
        var count = 0;

        try
        {
            await foreach (var file in context.GetInput().ReadFilesAsync(context.CancellationToken))
            {
                if (client is null)
                {
                    client = createClient();
                    await client.ConnectAsync(context.CancellationToken);
                    connected?.Invoke();

                    if (!string.IsNullOrEmpty(folder) && folder != "/")
                    {
                        await client.EnsureFolderAsync(folder, context.CancellationToken);
                    }
                }

                var remotePath = Combine(folder, file.FileName);

                if (!overwrite && await client.ExistsAsync(remotePath, context.CancellationToken))
                {
                    throw new DataPipelineDestinationException(exists(remotePath));
                }

                await using (var stream = file.OpenRead())
                {
                    await client.UploadAsync(stream, remotePath, overwrite, context.CancellationToken);
                }

                context.AddDelivery(describe(file.FileName, remotePath));
                count++;
            }
        }
        finally
        {
            if (client is not null)
            {
                await client.DisposeAsync();
            }
        }

        return count;
    }
}

/// <summary>
/// An error of a destination step whose message is meant for the user and holds no secrets.
/// </summary>
internal sealed class DataPipelineDestinationException : Exception
{
    public DataPipelineDestinationException()
    {
    }

    public DataPipelineDestinationException(string message)
        : base(message)
    {
    }

    public DataPipelineDestinationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
