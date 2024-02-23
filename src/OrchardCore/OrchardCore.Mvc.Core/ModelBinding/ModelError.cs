namespace OrchardCore.Mvc.ModelBinding
{
    public class ModelError
    {
        public ModelError(string key, string message)
        {
            Key = key;
            Message = message;
        }

        public string Key { get; }

        public string Message { get; }
    }
}
