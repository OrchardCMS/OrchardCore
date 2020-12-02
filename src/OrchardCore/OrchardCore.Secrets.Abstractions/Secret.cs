namespace OrchardCore.Secrets
{

    // Secrets are not stored as the class. The implementation of the Secret Store will
    // rehyrdate these as required by its implementation requirements.
    public abstract class Secret
    {

        // TODO Probably this class is bare, no identifier at all. We only care about the values.
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
