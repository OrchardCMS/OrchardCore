using System.Threading;

namespace Orchard.DisplayManagement.ModelBinding
{
    public class LocalModelBinderAccessor : IUpdateModelAccessor
    {
        private readonly AsyncLocal<IUpdateModel> _storage = new AsyncLocal<IUpdateModel>();

        public IUpdateModel ModelUpdater
        {
            get { return _storage.Value; }
            set { _storage.Value = value; }
        }
    }
}
