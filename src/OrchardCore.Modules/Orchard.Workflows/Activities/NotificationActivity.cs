using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Orchard.ContentManagement;
using Orchard.DisplayManagement.Notify;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;

namespace Orchard.Workflows.Activities
{
    public class NotificationActivity : TaskActivity
    {
        private readonly INotifier _notifier;

        private readonly IStringLocalizer S;
        private readonly IHtmlLocalizer H;

        public NotificationActivity(
            INotifier notifier,
            IStringLocalizer<NotificationActivity> s,
            IHtmlLocalizer<NotificationActivity> h)
        {
            _notifier = notifier;

            S = s;
            H = h;
        }

        public override string Name
        {
            get { return "Notify"; }
        }

        public override LocalizedString Category
        {
            get { return S["Notification"]; }
        }

        public override LocalizedString Description
        {
            get { return S["Display a message."]; }
        }

        public override string Form
        {
            get { return "ActivityNotify"; }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            yield return S["Done"];
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            var notification = activityContext.GetState<string>("Notification");
            var message = activityContext.GetState<string>("Message");

            NotifyType notificationType;
            Enum.TryParse(notification, true, out notificationType);
            _notifier.Add(notificationType, H[message]);

            yield return S["Done"];
        }
    }
}