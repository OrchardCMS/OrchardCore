using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.FileStorage;
using OrchardCore.FileStorage.FileSystem;
using OrchardCore.Media;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Email.Workflows.Activities
{
    public class EmailTask : TaskActivity
    {
        private readonly ISmtpService _smtpService;
        private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
        private readonly IStringLocalizer S;
        private readonly HtmlEncoder _htmlEncoder;
        private readonly IFileStore _fileStore;
        private readonly IOptions<ShellOptions> _shellOptions;
        readonly ShellSettings _shellSettings;

        public EmailTask(
            ISmtpService smtpService,
            IWorkflowExpressionEvaluator expressionEvaluator,
            IStringLocalizer<EmailTask> localizer,
            IMediaFileStore mediaFileStore,
            IOptions<ShellOptions> shellOptions,
            ShellSettings shellSettings,
            HtmlEncoder htmlEncoder
        )
        {
            _smtpService = smtpService;
            _expressionEvaluator = expressionEvaluator;
            S = localizer;
            _fileStore = mediaFileStore;
            _shellOptions = shellOptions;
            _shellSettings = shellSettings;
            _htmlEncoder = htmlEncoder;
        }

        public override string Name => nameof(EmailTask);
        public override LocalizedString DisplayText => S["Email Task"];
        public override LocalizedString Category => S["Messaging"];

        public WorkflowExpression<string> Author
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> Sender
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> ReplyTo
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        // TODO: Add support for the following format: Jack Bauer<jack@ctu.com>, ...
        public WorkflowExpression<string> Recipients
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> Cc
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> Bcc
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> Subject
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> Body
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> BodyText
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public bool IsBodyHtml
        {
            get => GetProperty(() => true);
            set => SetProperty(value);
        }

        public bool IsBodyText
        {
            get => GetProperty(() => true);
            set => SetProperty(value);
        }

        public bool IncludeStoredAttachments
        {
            get => GetProperty(() => true);
            set => SetProperty(value);
        }

        public bool RemoveStoredAttachments
        {
            get => GetProperty(() => true);
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Done"], S["Failed"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var author = await _expressionEvaluator.EvaluateAsync(Author, workflowContext, null);
            var sender = await _expressionEvaluator.EvaluateAsync(Sender, workflowContext, null);
            var replyTo = await _expressionEvaluator.EvaluateAsync(ReplyTo, workflowContext, null);
            var recipients = await _expressionEvaluator.EvaluateAsync(Recipients, workflowContext, null);
            var cc = await _expressionEvaluator.EvaluateAsync(Cc, workflowContext, null);
            var bcc = await _expressionEvaluator.EvaluateAsync(Bcc, workflowContext, null);
            var subject = await _expressionEvaluator.EvaluateAsync(Subject, workflowContext, null);
            var body = await _expressionEvaluator.EvaluateAsync(Body, workflowContext, _htmlEncoder);
            var bodyText = await _expressionEvaluator.EvaluateAsync(BodyText, workflowContext, null);
            IFileStore fileStore = null;

            var message = new MailMessage
            {
                // Author and Sender are both not required fields.
                From = author?.Trim() ?? sender?.Trim(),
                To = recipients?.Trim(),
                Cc = cc?.Trim(),
                Bcc = bcc?.Trim(),
                // Email reply-to header https://tools.ietf.org/html/rfc4021#section-2.1.4
                ReplyTo = replyTo?.Trim(),
                Subject = subject.Trim(),
                Body = body?.Trim(),
                BodyText = bodyText?.Trim(),
                IsBodyHtml = IsBodyHtml,
                IsBodyText = IsBodyText,
            };

            if (!String.IsNullOrWhiteSpace(sender))
            {
                message.Sender = sender.Trim();
            }

            fileStore = await AddMailAttachments(workflowContext, fileStore, message);
            var result = await _smtpService.SendAsync(message);
            await DeleteStoredAttachments(workflowContext, fileStore, result, message);

            workflowContext.LastResult = result;

            if (!result.Succeeded)
            {
                return Outcomes("Failed");
            }

            return Outcomes("Done");
        }

        private async Task DeleteStoredAttachments(WorkflowExecutionContext workflowContext, IFileStore fileStore, SmtpResult result, MailMessage message)
        {
            if (RemoveStoredAttachments && result.Succeeded && fileStore != null && workflowContext.Properties.ContainsKey("FormAttachments"))
            {
                // close open file streams
                message.Attachments.ForEach(p => p.Stream.Dispose());

                foreach (var filePath in (List<string>)workflowContext.Properties["FormAttachments"])
                {
                    var success = await fileStore?.TryDeleteFileAsync(filePath);
                }
            }
        }

        private async Task<IFileStore> AddMailAttachments(WorkflowExecutionContext workflowContext, IFileStore fileStore, MailMessage message)
        {
            if (IncludeStoredAttachments
                            && workflowContext.Properties.ContainsKey("FormAttachmentsUseMediaFileStore")
                            && workflowContext.Properties.ContainsKey("FormAttachments"))
            {
                bool useMediaFileStore = (bool)workflowContext.Properties["FormAttachmentsUseMediaFileStore"];
                fileStore = useMediaFileStore ? _fileStore : CreateDefaultFileStore();
                foreach (var filePath in (List<string>)workflowContext.Properties["FormAttachments"])
                {
                    var fileInfo = await fileStore.GetFileInfoAsync(filePath);
                    var fileStream = await fileStore.GetFileStreamAsync(fileInfo);
                    message.Attachments.Add(new MailMessageAttachment() { Filename = fileInfo.Name, Stream = fileStream });
                }
            }

            return fileStore;
        }

        private IFileStore CreateDefaultFileStore()
        {
            var shell = _shellOptions.Value;
            var innerPath = PathExtensions.Combine(shell.ShellsContainerName, _shellSettings.Name);
            var directory = PathExtensions.Combine(shell.ShellsApplicationDataPath, innerPath);
            return new FileSystemStore(directory);
        }
    }
}
