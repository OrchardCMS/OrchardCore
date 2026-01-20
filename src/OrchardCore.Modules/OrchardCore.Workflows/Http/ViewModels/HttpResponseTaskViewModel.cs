namespace OrchardCore.Workflows.Http.ViewModels
{
    public class HttpResponseTaskViewModel
    {
        public int HttpStatusCode { get; set; }
        public string Headers { get; set; }
        public string Content { get; set; }
        public string ContentType { get; set; }
    }
}
