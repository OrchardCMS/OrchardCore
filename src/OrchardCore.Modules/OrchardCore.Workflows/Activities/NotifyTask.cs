using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Activities
{
    public class NotifyTask : Activity
    {
        private readonly INotifier _notifier;
        private readonly IStringLocalizer S;
        private readonly IHtmlLocalizer H;

        public NotifyTask
        (
            INotifier notifier,
            IStringLocalizer<NotifyTask> s,
            IHtmlLocalizer<NotifyTask> h)
        {
            _notifier = notifier;

            S = s;
            H = h;
        }

        public override string Name => nameof(NotifyTask);
        public override LocalizedString Category => S["Notification"];
        public override LocalizedString Description => S["Display a message."];

        public NotifyType NotificationType
        {
            get => GetProperty<NotifyType>();
            set => SetProperty(value);
        }

        public string Message
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            yield return S["Done"];
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            _notifier.Add(NotificationType, H[Message]);
            yield return S["Done"];
        }
    }
}