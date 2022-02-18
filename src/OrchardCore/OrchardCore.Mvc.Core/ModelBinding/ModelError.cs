namespace OrchardCore.Mvc.ModelBinding
{
    public class ModelError
    {
        public ModelError()
        {

        }

        public ModelError(string key, string message)
        {
            Key = key;
            Message = message;
        }

        public string Key { get; set; }

        public string Message { get; set; }
    }
}
