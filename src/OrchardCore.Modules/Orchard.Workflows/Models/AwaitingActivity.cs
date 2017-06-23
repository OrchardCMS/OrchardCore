namespace Orchard.Workflows.Models {
    public class AwaitingActivity {
        public int Id { get; set; }

        public Activity Activity { get; set; }

        // Parent property
        public Workflow Workflow { get; set; }
    }
}