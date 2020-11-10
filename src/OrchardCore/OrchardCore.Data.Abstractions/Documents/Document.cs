namespace OrchardCore.Data.Documents
{
    public class Document : IDocument
    {
        /// <summary>
        /// The <see cref="IDocument.Identifier"/>.
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// Only defined to prevent a version missmatch, see https://github.com/sebastienros/yessql/pull/287
        /// </summary>
        public int Version { get; set; }
    }
}
