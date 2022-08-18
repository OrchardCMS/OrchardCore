using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.FileStorage;
using OrchardCore.FileStorage.FileSystem;
using OrchardCore.Media;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Activities
{
    public class SaveFormAttachmentsTask : TaskActivity
    {
        readonly IOptions<ShellOptions> _shellOptions;
        protected readonly IStringLocalizer S;
        readonly ShellSettings _shellSettings;
        private readonly IFileStore _fileStore;
        private readonly IHttpContextAccessor _http;
        private readonly IWorkflowExpressionEvaluator _expressionEvaluator;

        public SaveFormAttachmentsTask(
            IWorkflowExpressionEvaluator expressionEvaluator,
            IStringLocalizer<SaveFormAttachmentsTask> localizer,
            IOptions<ShellOptions> shellOptions,
            ShellSettings shellSettings,
            IMediaFileStore mediaFileStore,
            IHttpContextAccessor httpContextAccessor)
        {
            _shellOptions = shellOptions;
            _shellSettings = shellSettings;
            _fileStore = mediaFileStore;
            _http = httpContextAccessor;
            S = localizer;
            _expressionEvaluator = expressionEvaluator;
        }

        public override string Name => nameof(SaveFormAttachmentsTask);

        public override LocalizedString DisplayText => S["Save Form Attachments Task"];

        public override LocalizedString Category => S["Media"];

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Done"]);
        }

        public override bool CanExecute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return _http.HttpContext?.Request?.Form != null;
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            // use either configured mediaFileStore or create new FileSystemStore --> TODO: mediaFileStore does not resolve permissions, everything is in public folders and can be accessible.
            // to put files in different folder, we would need either to register some private MediaFileStore, or allow to specify folder outside base path (?)
            var fileStore = UseMediaFileStore ? _fileStore : CreateDefaultFileStore();
            var filePaths = new List<string>();
            foreach (var file in _http.HttpContext.Request.Form.Files)
            {
                var filePath = fileStore.Combine(Folder, $"{workflowContext.WorkflowId}-{file.FileName}");
                filePaths.Add(await fileStore.CreateFileFromStreamAsync(filePath, file.OpenReadStream()));
            }

            workflowContext.Properties["FormAttachments"] = filePaths;
            workflowContext.Properties["FormAttachmentsUseMediaFileStore"] = UseMediaFileStore;
            return Outcomes("Done");
        }

        public string Folder
        {
            get => GetProperty<string>(() => string.Empty);
            set => SetProperty(value);
        }

        public bool UseMediaFileStore
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        private IFileStore CreateDefaultFileStore()
        {
            var shell = _shellOptions.Value;
            var innerPath = PathExtensions.Combine(shell.ShellsContainerName, _shellSettings.Name);
            var directory = PathExtensions.Combine(shell.ShellsApplicationDataPath, innerPath);

            // do not include Folder in FileSystemStore path, it is included in file path.
            var destPath = PathExtensions.Combine(directory, Folder);
            Directory.CreateDirectory(destPath);
            return new FileSystemStore(directory);
        }
    }
}
