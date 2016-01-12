using System.Threading;

namespace Orchard.DisplayManagement.ModelBinding
{
    public class LocalModelBinderAccessor : IModelUpdaterAccessor
    {
        private readonly AsyncLocal<IModelUpdater> _storage = new AsyncLocal<IModelUpdater>();

        public IModelUpdater ModelUpdater
        {
            get { return _storage.Value; }
            set { _storage.Value = value; }
        }
    }
}
