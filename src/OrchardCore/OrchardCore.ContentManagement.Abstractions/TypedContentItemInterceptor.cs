using System;
using System.Collections.Generic;
using Castle.DynamicProxy;

namespace OrchardCore.ContentManagement
{
    public class TypedContentItemInterceptor<T> : IInterceptor where T : TypedContentItem
    {
        private const string _get = "get_";
        private const string _set = "set_";

        private static readonly IDictionary<string, Action<IInvocation, ContentItem>> _wellKnownProperties =
            new Dictionary<string, Action<IInvocation, ContentItem>>()
            {
                // ContentItem
                {
                    _get + nameof(ContentItem.ContentItem), new Action<IInvocation, ContentItem>((invocation, contentItem) =>
                    {
                        invocation.ReturnValue = contentItem.ContentItem;
                    })
                },
                {
                    _set + nameof(ContentItem.ContentItem), new Action<IInvocation, ContentItem>((invocation, contentItem) =>
                    {
                        contentItem.ContentItem = (ContentItem)invocation.GetArgumentValue(0);
                    })
                },
                // Id
                {
                    _get + nameof(ContentItem.Id), new Action<IInvocation, ContentItem>((invocation, contentItem) =>
                    {
                        invocation.ReturnValue = contentItem.Id;
                    })
                },
                {
                    _set + nameof(ContentItem.Id), new Action<IInvocation, ContentItem>((invocation, contentItem) =>
                    {
                        contentItem.Id = (int)invocation.GetArgumentValue(0);
                    })
                },
                // ContentItemId
                {
                    _get + nameof(ContentItem.ContentItemId), new Action<IInvocation, ContentItem>((invocation, contentItem) =>
                    {
                        invocation.ReturnValue = contentItem.ContentItemId;
                    })
                },
                {
                    _set + nameof(ContentItem.ContentItemId), new Action<IInvocation, ContentItem>((invocation, contentItem) =>
                    {
                        contentItem.ContentItemId = (string)invocation.GetArgumentValue(0);
                    })
                },
                // ContentItemVersionId
                {
                    _get + nameof(ContentItem.ContentItemVersionId), new Action<IInvocation, ContentItem>((invocation, contentItem) =>
                    {
                        invocation.ReturnValue = contentItem.ContentItemVersionId;
                    })
                },
                {
                    _set + nameof(ContentItem.ContentItemVersionId), new Action<IInvocation, ContentItem>((invocation, contentItem) =>
                    {
                        contentItem.ContentItemVersionId = (string)invocation.GetArgumentValue(0);
                    })
                },
                // ContentType
                {
                    _get + nameof(ContentItem.ContentType), new Action<IInvocation, ContentItem>((invocation, contentItem) =>
                    {
                        invocation.ReturnValue = contentItem.ContentType;
                    })
                },
                {
                    _set + nameof(ContentItem.ContentType), new Action<IInvocation, ContentItem>((invocation, contentItem) =>
                    {
                        contentItem.ContentItemVersionId = (string)invocation.GetArgumentValue(0);
                    })
                },
                // Published
                {
                    _get + nameof(ContentItem.Published), new Action<IInvocation, ContentItem>((invocation, contentItem) =>
                    {
                        invocation.ReturnValue = contentItem.Published;
                    })
                },
                {
                    _set + nameof(ContentItem.Published), new Action<IInvocation, ContentItem>((invocation, contentItem) =>
                    {
                        contentItem.Published = (bool)invocation.GetArgumentValue(0);
                    })
                },
                // Latest
                {
                    _get + nameof(ContentItem.Latest), new Action<IInvocation, ContentItem>((invocation, contentItem) =>
                    {
                        invocation.ReturnValue = contentItem.Latest;
                    })
                },
                {
                    _set + nameof(ContentItem.Latest), new Action<IInvocation, ContentItem>((invocation, contentItem) =>
                    {
                        contentItem.Latest = (bool)invocation.GetArgumentValue(0);
                    })
                },
                // ModifiedUtc
                {
                    _get + nameof(ContentItem.ModifiedUtc), new Action<IInvocation, ContentItem>((invocation, contentItem) =>
                    {
                        invocation.ReturnValue = contentItem.ModifiedUtc;
                    })
                },
                {
                    _set + nameof(ContentItem.ModifiedUtc), new Action<IInvocation, ContentItem>((invocation, contentItem) =>
                    {
                        contentItem.ModifiedUtc = (DateTime?)invocation.GetArgumentValue(0);
                    })
                },
                // PublishedUtc
                {
                    _get + nameof(ContentItem.PublishedUtc), new Action<IInvocation, ContentItem>((invocation, contentItem) =>
                    {
                        invocation.ReturnValue = contentItem.PublishedUtc;
                    })
                },
                {
                    _set + nameof(ContentItem.PublishedUtc), new Action<IInvocation, ContentItem>((invocation, contentItem) =>
                    {
                        contentItem.PublishedUtc = (DateTime?)invocation.GetArgumentValue(0);
                    })
                },
                // CreatedUtc
                {
                    _get + nameof(ContentItem.CreatedUtc), new Action<IInvocation, ContentItem>((invocation, contentItem) =>
                    {
                        invocation.ReturnValue = contentItem.CreatedUtc;
                    })
                },
                {
                    _set + nameof(ContentItem.CreatedUtc), new Action<IInvocation, ContentItem>((invocation, contentItem) =>
                    {
                        contentItem.CreatedUtc = (DateTime?)invocation.GetArgumentValue(0);
                    })
                },
                // Owner
                {
                    _get + nameof(ContentItem.Owner), new Action<IInvocation, ContentItem>((invocation, contentItem) =>
                    {
                        invocation.ReturnValue = contentItem.Owner;
                    })
                },
                {
                    _set + nameof(ContentItem.Owner), new Action<IInvocation, ContentItem>((invocation, contentItem) =>
                    {
                        contentItem.Owner = (string)invocation.GetArgumentValue(0);
                    })
                },
                // Author
                {
                    _get + nameof(ContentItem.Author), new Action<IInvocation, ContentItem>((invocation, contentItem) =>
                    {
                        invocation.ReturnValue = contentItem.Author;
                    })
                },
                {
                    _set + nameof(ContentItem.Author), new Action<IInvocation, ContentItem>((invocation, contentItem) =>
                    {
                        contentItem.Author = (string)invocation.GetArgumentValue(0);
                    })
                },
                // DisplayText
                {
                    _get + nameof(ContentItem.DisplayText), new Action<IInvocation, ContentItem>((invocation, contentItem) =>
                    {
                        invocation.ReturnValue = contentItem.DisplayText;
                    })
                },
                {
                    _set + nameof(ContentItem.DisplayText), new Action<IInvocation, ContentItem>((invocation, contentItem) =>
                    {
                        contentItem.DisplayText = (string)invocation.GetArgumentValue(0);
                    })
                },
            };

        public void Intercept(IInvocation invocation)
        {
            var contentItem = ((T)invocation.InvocationTarget).ContentItem;

            // Return well known properties.
            if (_wellKnownProperties.TryGetValue(invocation.Method.Name, out var action))
            {
                action.Invoke(invocation, contentItem);

            }
            else if (invocation.Method.Name.StartsWith(_get, StringComparison.OrdinalIgnoreCase) &&
                invocation.MethodInvocationTarget.ReturnType.IsSubclassOf(typeof(ContentElement)))
            {
                var methodName = invocation.Method.Name[4..];
                if (invocation.MethodInvocationTarget.ReturnType.IsSubclassOf(typeof(ContentPart)))
                {
                    // Tries to retrieve content elements from the content item.
                    invocation.ReturnValue = contentItem.Get(invocation.MethodInvocationTarget.ReturnType, methodName);
                } else if (invocation.MethodInvocationTarget.ReturnType.IsSubclassOf(typeof(ContentField))){
                    var fieldContentPart = contentItem.Get(typeof(ContentPart), contentItem.ContentType);
                    invocation.ReturnValue = fieldContentPart.Get(invocation.MethodInvocationTarget.ReturnType, methodName);
                } else
                {
                    throw new Exception("Unknown content element type");
                }
            }
            else if (invocation.Method.Name.StartsWith(_set, StringComparison.OrdinalIgnoreCase) &&
                invocation.GetArgumentValue(0) is ContentElement ce)
            {
                throw new Exception("Content items or parts should be assigned with GetOrCreate");
            }
            else
            {
                // Property is not related to a ContentItem, Part, or Field, proceed as normal.
                invocation.Proceed();
            }
        }
    }
}
