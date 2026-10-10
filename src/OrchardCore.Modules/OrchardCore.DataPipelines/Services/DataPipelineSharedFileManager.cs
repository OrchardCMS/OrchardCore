using System.Globalization;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.DataPipelines.Indexes;
using OrchardCore.DataPipelines.Models;
using OrchardCore.DataPipelines.Steps;
using OrchardCore.Email;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using YesSql;
using IIdGenerator = OrchardCore.Entities.IIdGenerator;
using ISession = YesSql.ISession;

namespace OrchardCore.DataPipelines.Services;

/// <summary>
/// Keeps the files shared through download links in the tenant's App_Data folder, out of the media library, and
/// serves them to their recipients only.
/// </summary>
public sealed class DataPipelineSharedFileManager : IDataPipelineSharedFileManager
{
    private readonly UserManager<IUser> _userManager;
    private readonly ISession _session;
    private readonly IClock _clock;
    private readonly IIdGenerator _idGenerator;
    private readonly IOptions<ShellOptions> _shellOptions;
    private readonly ShellSettings _shellSettings;
    private readonly LinkGenerator _linkGenerator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceProvider _services;
    private readonly HtmlEncoder _htmlEncoder;
    private readonly IStringLocalizer S;

    public DataPipelineSharedFileManager(
        UserManager<IUser> userManager,
        ISession session,
        IClock clock,
        IIdGenerator idGenerator,
        IOptions<ShellOptions> shellOptions,
        ShellSettings shellSettings,
        LinkGenerator linkGenerator,
        IHttpContextAccessor httpContextAccessor,
        IServiceProvider services,
        HtmlEncoder htmlEncoder,
        IStringLocalizer<DataPipelineSharedFileManager> localizer)
    {
        _userManager = userManager;
        _session = session;
        _clock = clock;
        _idGenerator = idGenerator;
        _shellOptions = shellOptions;
        _shellSettings = shellSettings;
        _linkGenerator = linkGenerator;
        _httpContextAccessor = httpContextAccessor;
        _services = services;
        _htmlEncoder = htmlEncoder;
        S = localizer;
    }

    public async Task<DataPipelineSharedFileResult> ShareAsync(DataPipelineFile file, DataPipelineSharedFileRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentNullException.ThrowIfNull(request);

        var recipients = new List<User>();

        foreach (var recipient in request.Recipients)
        {
            var user = await _userManager.FindByNameAsync(recipient) ?? await _userManager.FindByEmailAsync(recipient);

            if (user is not User found)
            {
                throw new InvalidOperationException(S["There is no user named '{0}', or with that email.", recipient]);
            }

            if (recipients.All(item => item.UserId != found.UserId))
            {
                recipients.Add(found);
            }
        }

        var fileId = _idGenerator.GenerateUniqueId();
        var directory = GetDirectory(fileId);
        Directory.CreateDirectory(directory);

        await using (var source = file.OpenRead())
        await using (var target = new FileStream(Path.Combine(directory, Path.GetFileName(file.FileName)), FileMode.CreateNew, FileAccess.Write, FileShare.None, 16 * 1024, FileOptions.Asynchronous))
        {
            await source.CopyToAsync(target, cancellationToken);
        }

        var token = DataPipelineSharedFileAccess.CreateToken();
        var now = _clock.UtcNow;
        var sharedFile = new DataPipelineSharedFile
        {
            FileId = fileId,
            PipelineId = request.Run?.PipelineId,
            PipelineName = request.Run?.PipelineName,
            RunId = request.Run?.RunId,
            FileName = Path.GetFileName(file.FileName),
            ContentType = file.ContentType,
            Length = file.Length,
            TokenHash = DataPipelineSharedFileAccess.Hash(token),
            RecipientUserIds = recipients.Select(user => user.UserId).ToList(),
            RecipientNames = recipients.Select(user => user.UserName).ToList(),
            CreatedUtc = now,
            ExpiresUtc = now.Add(request.Lifetime),
        };

        await _session.SaveAsync(sharedFile, cancellationToken: cancellationToken);

        var url = GetUrl(fileId, token);
        var notified = request.NotifyByEmail
            ? await NotifyAsync(recipients, sharedFile, url, request, cancellationToken)
            : [];

        return new DataPipelineSharedFileResult
        {
            SharedFile = sharedFile,
            Url = url,
            Notified = notified,
        };
    }

    /// <summary>
    /// Opens the content of a shared file.
    /// </summary>
    /// <param name="sharedFile">The shared file.</param>
    /// <returns>The content, or <see langword="null"/> when it was deleted.</returns>
    public Stream OpenRead(DataPipelineSharedFile sharedFile)
    {
        ArgumentNullException.ThrowIfNull(sharedFile);

        var path = Path.Combine(GetDirectory(sharedFile.FileId), Path.GetFileName(sharedFile.FileName));

        return File.Exists(path) ? new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 16 * 1024, FileOptions.Asynchronous) : null;
    }

    /// <summary>
    /// Finds a shared file.
    /// </summary>
    /// <param name="fileId">The identifier of the shared file.</param>
    /// <returns>The shared file, or <see langword="null"/>.</returns>
    public async Task<DataPipelineSharedFile> GetAsync(string fileId)
    {
        if (string.IsNullOrEmpty(fileId))
        {
            return null;
        }

        return await _session.Query<DataPipelineSharedFile, DataPipelineSharedFileIndex>(index => index.FileId == fileId).FirstOrDefaultAsync();
    }

    /// <summary>
    /// Lists the shared files, the most recent first.
    /// </summary>
    /// <param name="skip">The number of files to skip.</param>
    /// <param name="take">The number of files to return.</param>
    /// <returns>The files and their number.</returns>
    public async Task<(IReadOnlyList<DataPipelineSharedFile> Files, int Count)> ListAsync(int skip, int take)
    {
        var query = _session.Query<DataPipelineSharedFile, DataPipelineSharedFileIndex>();
        var count = await query.CountAsync();
        var files = await query.OrderByDescending(index => index.CreatedUtc).Skip(skip).Take(take).ListAsync();

        return (files.ToList(), count);
    }

    /// <summary>
    /// Saves a shared file, such as after revoking its link.
    /// </summary>
    /// <param name="sharedFile">The shared file.</param>
    /// <returns>A task that completes when the file is saved.</returns>
    public Task SaveAsync(DataPipelineSharedFile sharedFile)
        => _session.SaveAsync(sharedFile);

    /// <summary>
    /// Lists the shared files whose link stopped working before a date.
    /// </summary>
    /// <param name="before">The date, in UTC.</param>
    /// <param name="take">The number of files to return.</param>
    /// <returns>The files.</returns>
    public async Task<IReadOnlyList<DataPipelineSharedFile>> ListExpiredAsync(DateTime before, int take)
        => (await _session.Query<DataPipelineSharedFile, DataPipelineSharedFileIndex>(index => index.ExpiresUtc < before).Take(take).ListAsync()).ToList();

    /// <summary>
    /// Deletes a shared file, its content and its link.
    /// </summary>
    /// <param name="sharedFile">The shared file.</param>
    public void Delete(DataPipelineSharedFile sharedFile)
    {
        ArgumentNullException.ThrowIfNull(sharedFile);

        var directory = GetDirectory(sharedFile.FileId);

        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }

        _session.Delete(sharedFile);
    }

    private string GetDirectory(string fileId)
        => Path.Combine(_shellOptions.Value.ShellsApplicationDataPath, _shellOptions.Value.ShellsContainerName, _shellSettings.Name, "DataPipelines", "SharedFiles", Path.GetFileName(fileId));

    private string GetUrl(string fileId, string token)
    {
        var values = new RouteValueDictionary { ["area"] = "OrchardCore.DataPipelines", ["fileId"] = fileId, ["token"] = token };
        var httpContext = _httpContextAccessor.HttpContext;

        return (httpContext is null ? null : _linkGenerator.GetUriByAction(httpContext, "Download", "SharedFile", values))
            ?? _linkGenerator.GetPathByAction("Download", "SharedFile", values);
    }

    private async Task<IReadOnlyList<string>> NotifyAsync(List<User> recipients, DataPipelineSharedFile sharedFile, string url, DataPipelineSharedFileRequest request, CancellationToken cancellationToken)
    {
        var emailService = _services.GetService<IEmailService>();

        if (emailService is null)
        {
            return [];
        }

        var now = request.Run is null ? _clock.UtcNow : await DataPipelineFormulas.GetRunTimeAsync(request.Run);
        var subject = request.Run is null ? request.Subject : DataPipelineTemplate.Render(request.Subject, request.Run, now);
        var body = $"""
            <p>{_htmlEncoder.Encode(S["The file '{0}' was shared with you.", sharedFile.FileName])}</p>
            {(string.IsNullOrWhiteSpace(request.Message) ? string.Empty : $"<p>{_htmlEncoder.Encode(request.Message)}</p>")}
            <p><a href="{_htmlEncoder.Encode(url)}">{_htmlEncoder.Encode(S["Download {0}", sharedFile.FileName])}</a></p>
            <p>{_htmlEncoder.Encode(S["Sign in to download it. The link works until {0} (UTC).", sharedFile.ExpiresUtc.ToString("f", CultureInfo.CurrentCulture)])}</p>
            """;

        var notified = new List<string>();

        foreach (var recipient in recipients.Where(user => !string.IsNullOrEmpty(user.Email)))
        {
            var result = await emailService.SendAsync(new MailMessage
            {
                To = recipient.Email,
                Subject = string.IsNullOrWhiteSpace(subject) ? S["A file is ready"] : subject,
                HtmlBody = body,
            }, cancellationToken: cancellationToken);

            if (result.Succeeded)
            {
                notified.Add(recipient.UserName);
            }
        }

        return notified;
    }
}
