using System;

namespace OrchardCore.Demo.Models
{
    public class TodoModel
    {
        public string TodoId { get; set; }
        public string Text { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsCompleted { get; set; }
    }
}
