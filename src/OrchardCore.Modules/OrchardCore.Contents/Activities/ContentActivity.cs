using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Entities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Activities
{
    public abstract class ContentEvent : Event
    {
        protected readonly IStringLocalizer S;

        public ContentEvent(IStringLocalizer s)
        {
            S = s;
        }

        public override bool CanStartWorkflow => true;

        public override bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            try
            {

                var contentTypesState = activityContext.Activity.As<string>("ContentTypes");

                // "" means 'any'.
                if (string.IsNullOrEmpty(contentTypesState))
                {
                    return true;
                }

                var contentTypes = contentTypesState.Split(',');
                var content = workflowContext.WorkflowInstance.State.As<IContent>();

                if (content == null)
                {
                    return false;
                }

                return contentTypes.Any(contentType => content.ContentItem.ContentType == contentType);
            }
            catch
            {
                return false;
            }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return new[] { S["Done"] };
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            yield return S["Done"];
        }

        public override string Form
        {
            get
            {
                return "SelectContentTypes";
            }
        }

        public override LocalizedString Category
        {
            get { return S["Content Items"]; }
        }
    }

    public class ContentCreatedActivity : ContentActivity
    {
        public ContentCreatedActivity(IStringLocalizer<ContentCreatedActivity> s) : base(s)
        {
        }

        public override string Name
        {
            get { return "ContentCreated"; }
        }

        public override LocalizedString Description
        {
            get { return S["Content is created."]; }
        }
    }

    public class ContentUpdatedActivity : ContentActivity
    {
        public ContentUpdatedActivity(IStringLocalizer<ContentUpdatedActivity> s) : base(s)
        {
        }

        public override string Name
        {
            get { return "ContentUpdated"; }
        }

        public override LocalizedString Description
        {
            get { return S["Content is updated."]; }
        }
    }

    public class ContentPublishedActivity : ContentActivity
    {
        public ContentPublishedActivity(IStringLocalizer<ContentPublishedActivity> s) : base(s)
        {
        }

        public override string Name
        {
            get { return "ContentPublished"; }
        }


        public override LocalizedString Description
        {
            get { return S["Content is published."]; }
        }
    }

    public class ContentVersionedActivity : ContentActivity
    {
        public ContentVersionedActivity(IStringLocalizer<ContentVersionedActivity> s) : base(s)
        {
        }

        public override string Name
        {
            get { return "ContentVersioned"; }
        }


        public override LocalizedString Description
        {
            get { return S["Content is versioned."]; }
        }
    }

    public class ContentRemovedActivity : ContentActivity
    {
        public ContentRemovedActivity(IStringLocalizer<ContentRemovedActivity> s) : base(s)
        {
        }

        public override string Name
        {
            get { return "ContentRemoved"; }
        }

        public override LocalizedString Description
        {
            get { return S["Content is removed."]; }
        }
    }
}