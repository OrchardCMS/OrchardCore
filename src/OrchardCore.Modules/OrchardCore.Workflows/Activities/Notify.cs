using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Activities
{
    public class Notify : Services.Activity
    {
        private readonly INotifier _notifier;
        private readonly IStringLocalizer S;
        private readonly IHtmlLocalizer H;

        public Notify(
            INotifier notifier,
            IStringLocalizer<Notify> s,
            IHtmlLocalizer<Notify> h)
        {
            _notifier = notifier;

            S = s;
            H = h;
        }

        public override string Name => nameof(Notify);
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