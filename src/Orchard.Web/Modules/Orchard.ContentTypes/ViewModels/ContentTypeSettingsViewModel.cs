namespace Orchard.ContentTypes.ViewModels {
    public class ContentTypeSettingsViewModel {
        public bool Creatable { get; set; }
        public bool Listable { get; set; }
        public bool Draftable { get; set; }
        public bool Securable { get; set; }
        public string Stereotype { get; set; }
    }
}