namespace OrchardCore.Workflows.Models
{
    public class SerializeWorkflowValueContext
    {
        public SerializeWorkflowValueContext(object input)
        {
            Input = input;
            Output = input;
        }

        public object Input { get; set; }
        public object Output { get; set; }
    }
}
