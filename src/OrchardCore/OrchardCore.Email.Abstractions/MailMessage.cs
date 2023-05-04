using System;
using System.Collections.Generic;

namespace OrchardCore.Email
{
    /// <summary>
    /// Represents a class that contains information of the mail message.
    /// </summary>
    public class MailMessage
    {
        /// <summary>
        /// Gets or sets the author of the email.
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// Gets or sets the recipients.
        /// </summary>
        public string To { get; set; }

        /// <summary>
        /// Gets or sets the carbon copy emails.
        /// </summary>
        public string Cc { get; set; }

        /// <summary>
        /// Gets or sets a blind copy emails.
        /// </summary>
        public string Bcc { get; set; }

        /// <summary>
        /// Gets or sets the replied to emails.
        /// </summary>
        public string ReplyTo { get; set; }

        /// <summary>
        /// Gets or sets the actual submitter of the email.
        /// </summary>
        /// <remark>
        /// This property is required if not the same as <see cref="From"/>, for more information please refer to https://ietf.org/rfc/rfc822.txt.
        /// </remark>
        public string Sender { get; set; }

        /// <summary>
        /// Gets or sets the message subject.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the message content aka body.
        /// </summary>
        /// <remarks>This property is work in conjunction with <see cref="IsHtmlBody"/> to determine the body type..</remarks>
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets the message content as plain text.
        /// </summary>
        [Obsolete("This property is deprecated, please use Body instead.")]
        public string BodyText
        {
            get => Body;
            set
            {
                Body = value;
                IsHtmlBody = false;
            }
        }

        /// <summary>
        /// Gets or sets whether the message body is an HTML.
        /// </summary>
        [Obsolete("This property is deprecated, please use IsHtmlBody instead.")]
        public bool IsBodyHtml
        {
            get => IsHtmlBody;
            set => IsHtmlBody = value;
        }

        /// <summary>
        /// Gets or sets whether the message body is plain text.
        /// </summary>
        [Obsolete("This property is deprecated, please use IsHtmlBody instead.")]
        public bool IsBodyText
        {
            get => !IsHtmlBody;
            set => IsHtmlBody = !value;
        }

        /// <summary>
        /// Gets or sets whether the message body is an HTML or not. Default is <c>false</c> which is plain text.
        /// </summary>
        public bool IsHtmlBody { get; set; }

        /// <summary>
        /// The collection of message attachments.
        /// </summary>
        public List<MailMessageAttachment> Attachments { get; } = new List<MailMessageAttachment>();
    }
}
