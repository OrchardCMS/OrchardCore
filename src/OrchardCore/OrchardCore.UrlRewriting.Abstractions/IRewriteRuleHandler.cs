using OrchardCore.UrlRewriting.Models;

namespace OrchardCore.UrlRewriting;

public interface IRewriteRuleHandler
{
    /// <summary>
    /// This method in invoked during rule initializing.
    /// </summary>
    /// <param name="context">An instance of <see cref="InitializingRewriteRuleContext"/>.</param>
    Task InitializingAsync(InitializingRewriteRuleContext context);

    /// <summary>
    /// This method in invoked after the rule was initialized.
    /// </summary>
    /// <param name="context">An instance of <see cref="InitializedRewriteRuleContext"/>.</param>
    Task InitializedAsync(InitializedRewriteRuleContext context);

    /// <summary>
    /// This method in invoked after the rule was loaded from the store.
    /// </summary>
    /// <param name="context">An instance of <see cref="LoadedRewriteRuleContext"/>.</param>
    Task LoadedAsync(LoadedRewriteRuleContext context);

    /// <summary>
    /// This method in invoked during rule removing.
    /// </summary>
    /// <param name="context">An instance of <see cref="DeletingRewriteRuleContext"/>.</param>
    Task DeletingAsync(DeletingRewriteRuleContext context);

    /// <summary>
    /// This method in invoked after the rule was removed.
    /// </summary>
    /// <param name="context">An instance of <see cref="DeletedRewriteRuleContext"/>.</param>
    Task DeletedAsync(DeletedRewriteRuleContext context);

    /// <summary>
    /// This method in invoked during rule saving.
    /// </summary>
    /// <param name="context">An instance of <see cref="SavingRewriteRuleContext"/>.</param>
    Task SavingAsync(SavingRewriteRuleContext context);

    /// <summary>
    /// This method in invoked after the rule was saved.
    /// </summary>
    /// <param name="context">An instance of <see cref="SavedRewriteRuleContext"/>.</param>
    Task SavedAsync(SavedRewriteRuleContext context);
}
