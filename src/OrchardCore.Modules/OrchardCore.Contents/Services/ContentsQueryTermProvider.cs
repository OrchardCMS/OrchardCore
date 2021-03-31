using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents.Security;
using OrchardCore.Contents.ViewModels;
using OrchardCore.Data.QueryParser;
using OrchardCore.DisplayManagement.ModelBinding;
using YesSql;
using YesSql.Services;
using static OrchardCore.Data.QueryParser.Fluent.QueryParsers;

namespace OrchardCore.Contents.Services
{
    public class ContentsQueryTermProvider : ITermParserProvider<ContentItem>
    {
        public IEnumerable<TermParser<ContentItem>> GetTermParsers()
            => new TermParser<ContentItem>[] 
            {
                NamedTermParser("status",
                    OneConditionParser<ContentItem>((query, val) =>
                    {
                        if (Enum.TryParse<ContentsStatus>(val, true, out var e))
                        {
                            switch (e)
                            {
                                case ContentsStatus.Published:
                                    query.With<ContentItemIndex>(x => x.Published);
                                    break;
                                case ContentsStatus.Draft:
                                    query.With<ContentItemIndex>(x => x.Latest && !x.Published);
                                    break;
                                case ContentsStatus.AllVersions:
                                    query.With<ContentItemIndex>(x => x.Latest);
                                    break;
                                default:
                                    query.With<ContentItemIndex>(x => x.Latest);
                                    break;
                            }
                        }

                        return query;
                    })
                ),
                NamedTermParser("sort",
                    OneConditionParser<ContentItem>((query, val) =>
                    {
                        // TODO we can also support -asc and -desc here.

                        var values = val.Split('-', 2);

                        var enumVal = values[0];
                        bool ascending = false;
                        if (values.Length == 2 && values[1] == "asc")
                        {
                            ascending = true;
                        }

                        if (Enum.TryParse<ContentsOrder>(val, true, out var e))
                        {
                            switch (e)
                            {
                                case ContentsOrder.Modified:
                                    if (ascending) 
                                    {
                                        query.With<ContentItemIndex>().OrderBy(x => x.ModifiedUtc);
                                    } 
                                    else 
                                    {
                                        query.With<ContentItemIndex>().OrderByDescending(x => x.ModifiedUtc);
                                    }
                                    break;
                                case ContentsOrder.Published:
                                    if (ascending) 
                                    {
                                        query.With<ContentItemIndex>().OrderBy(cr => cr.PublishedUtc);
                                    }
                                    else
                                    { 
                                        query.With<ContentItemIndex>().OrderByDescending(cr => cr.PublishedUtc);
                                    }
                                    break;
                                case ContentsOrder.Created:
                                    if (ascending) 
                                    {
                                       query.With<ContentItemIndex>().OrderBy(cr => cr.CreatedUtc);
                                    }
                                    else
                                    {
                                        query.With<ContentItemIndex>().OrderByDescending(cr => cr.CreatedUtc);
                                    }
                                    break;
                                case ContentsOrder.Title:
                                // todo support ascending.
                                    query.With<ContentItemIndex>().OrderBy(cr => cr.DisplayText);
                                    break;

                                // todo add Default to the enum, or make it always modified. investigate.
                                default:
                                    query.With<ContentItemIndex>().OrderByDescending(cr => cr.ModifiedUtc);
                                    break;
                            };
                        }

                        return query;
                    })
                ),
                DefaultTermParser("text",
                        ManyConditionParser<ContentItem>(
                            ((query, val) => query.With<ContentItemIndex>(x => x.DisplayText.Contains(val))),
                            ((query, val) => query.With<ContentItemIndex>(x => x.DisplayText.IsNotIn<ContentItemIndex>(s => s.DisplayText, w => w.DisplayText.Contains(val))))
                        )
                    )                
            };
    }
}
