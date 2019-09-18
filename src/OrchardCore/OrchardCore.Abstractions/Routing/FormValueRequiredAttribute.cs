namespace OrchardCore.Routing
{
    public class FormValueRequiredAttribute : System.Attribute
    {
        public FormValueRequiredAttribute(string formKey)
        {
            FormKey = formKey;
        }

        public string FormKey { get; }
    }
}
