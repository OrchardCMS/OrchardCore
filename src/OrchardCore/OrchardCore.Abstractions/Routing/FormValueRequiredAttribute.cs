using System;

namespace OrchardCore.Routing
{
    [AttributeUsage(AttributeTargets.Method)]
    public class FormValueRequiredAttribute : Attribute
    {
        public FormValueRequiredAttribute(string formKey)
        {
            FormKey = formKey;
        }

        public string FormKey { get; }
    }
}
