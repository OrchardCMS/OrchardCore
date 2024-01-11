using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OrchardCore;
using OrchardCore.ContentManagement;
using OrchardCore.Taxonomies.Indexing;
using YesSql;

#pragma warning disable CA1050 // Declare types in namespaces
public static class TaxonomyOrchardHelperExtensions
#pragma warning restore CA1050 // Declare types in namespaces
{
    /// <summary>
    /// Returns a term from its content item id and taxonomy.
    /// </summary>
    /// <param name="orchardHelper">The <see cref="IOrchardHelper"/>.</param>
    /// <param name="taxonomyContentItemId">The taxonomy content item id.</param>
    /// <param name="termContentItemId">The term content item id.</param>
    /// <returns>A content item id <c>null</c> if it was not found.</returns>
    public static async Task<ContentItem> GetTaxonomyTermAsync(this IOrchardHelper orchardHelper, string taxonomyContentItemId, string termContentItemId)
    {
        var contentManager = orchardHelper.HttpContext.RequestServices.GetService<IContentManager>();
        var taxonomy = await contentManager.GetAsync(taxonomyContentItemId);

        if (taxonomy == null)
        {
            return null;
        }

        return FindTerm(taxonomy.Content.TaxonomyPart.Terms as JArray, termContentItemId);
    }

    /// <summary>
    /// Returns the list of terms including their parents.
    /// </summary>
    /// <param name="orchardHelper">The <see cref="IOrchardHelper"/>.</param>
    /// <param name="taxonomyContentItemId">The taxonomy content item id.</param>
    /// <param name="termContentItemId">The term content item id.</param>
    /// <returns>A list content items.</returns>
    public static async Task<List<ContentItem>> GetInheritedTermsAsync(this IOrchardHelper orchardHelper, string taxonomyContentItemId, string termContentItemId)
    {
        var contentManager = orchardHelper.HttpContext.RequestServices.GetService<IContentManager>();
        var taxonomy = await contentManager.GetAsync(taxonomyContentItemId);

        if (taxonomy == null)
        {
            return null;
        }

        var terms = new List<ContentItem>();

        FindTermHierarchy(taxonomy.Content.TaxonomyPart.Terms as JArray, termContentItemId, terms);

        return terms;
    }

    /// <summary>
    /// Query content items.
    /// </summary>
    public static async Task<IEnumerable<ContentItem>> QueryCategorizedContentItemsAsync(this IOrchardHelper orchardHelper, Func<IQuery<ContentItem, TaxonomyIndex>, IQuery<ContentItem>> query)
    {
        var contentManager = orchardHelper.HttpContext.RequestServices.GetService<IContentManager>();
        var session = orchardHelper.HttpContext.RequestServices.GetService<ISession>();

        var contentItems = await query(session.Query<ContentItem, TaxonomyIndex>()).ListAsync();

        return await contentManager.LoadAsync(contentItems);
    }

    internal static ContentItem FindTerm(JArray termsArray, string termContentItemId)
    {
        foreach (var term in termsArray.Cast<JObject>())
        {
            var contentItemId = term.GetValue("ContentItemId").ToString();

            if (contentItemId == termContentItemId)
            {
                return term.ToObject<ContentItem>();
            }

            if (term.GetValue("Terms") is JArray children)
            {
                var found = FindTerm(children, termContentItemId);

                if (found != null)
                {
                    return found;
                }
            }
        }

        return null;
    }

    internal static bool FindTermHierarchy(JArray termsArray, string termContentItemId, List<ContentItem> terms)
    {
        foreach (var term in termsArray.Cast<JObject>())
        {
            var contentItemId = term.GetValue("ContentItemId").ToString();

            if (contentItemId == termContentItemId)
            {
                terms.Add(term.ToObject<ContentItem>());

                return true;
            }

            if (term.GetValue("Terms") is JArray children)
            {
                var found = FindTermHierarchy(children, termContentItemId, terms);

                if (found)
                {
                    terms.Add(term.ToObject<ContentItem>());

                    return true;
                }
            }
        }

        return false;
    }
}
