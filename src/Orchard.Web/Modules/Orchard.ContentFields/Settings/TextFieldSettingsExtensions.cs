using System;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Orchard.ContentManagement.MetaData.Builders;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;

namespace Orchard.ContentFields.Settings
{
    public static class TextFieldSettingsExtensions
    {
        public static ContentPartFieldDefinitionBuilder Hint(this ContentPartFieldDefinitionBuilder builder, string hint)
        {
            return builder.WithSetting("Hint", hint);
        }

        public static string FieldNameFor<T, TResult>(this HtmlHelper<T> html, Expression<Func<T, TResult>> expression)
        {
            return html.ViewData.TemplateInfo.GetFullHtmlFieldName(ExpressionHelper.GetExpressionText(expression));
        }

        //public static string FieldIdFor<T, TResult>(this HtmlHelper<T> html, Expression<Func<T, TResult>> expression)
        //{
        //    var id = html.ViewData.TemplateInfo.GetFullHtmlFieldId(ExpressionHelper.GetExpressionText(expression));
        //    // because "[" and "]" aren't replaced with "_" in GetFullHtmlFieldId
        //    return id.Replace('[', '_').Replace(']', '_');
        //}
    }
}
