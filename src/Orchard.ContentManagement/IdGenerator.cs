namespace Orchard.ContentManagement
{
    public class IdGenerator
    {
        public string GenerateUniqueId()
        {
            return System.Guid.NewGuid().ToString("n");
        }
    }
}
