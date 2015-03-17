using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using OrchardVNext.ContentManagement.Handlers;
using OrchardVNext.ContentManagement.Records;

namespace OrchardVNext.Data.EF {
    public interface IContentQueryExpressionHandler : IDependency {
        Expression<Func<TContent, bool>> OnCreating<TContent>() where TContent : DocumentRecord;
    }

    //public abstract class ContentQueryExpressionHandler : IContentQueryExpressionHandler {
    //    protected ContentQueryExpressionHandler() {
    //        Filters = new List<IContentQueryExpressionHandler>();
    //    }

    //    public List<IContentQueryExpressionHandler> Filters { get; set; }
    //    protected void OnActivated<TPart>(Action<s, TPart> handler) where TPart : DocumentRecord {
    //        Filters.Add(new InlineStorageFilter<TPart> { OnActivated = handler });
    //    }

    //    Expression<Func<TContent, bool>> IContentQueryExpressionHandler.OnCreating<TContent>()
    //    {
    //        return c => c.Data == "";
    //    }
    //}

    //public class ContentQueryExpressionHandlerImpl : ContentQueryExpressionHandler
    //{
    //    public ContentQueryExpressionHandlerImpl()
    //    {
    //        OnCreating<>
    //    }
    //}
}