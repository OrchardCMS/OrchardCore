using Orchard.DependencyInjection;
using System.Collections.Concurrent;

namespace Orchard.DisplayManagement.Descriptors {

    /// <summary>
    /// Will return the same shape table per name within the same unit of work.
    /// Reduces compute costs compared to IShapeTableManager.GetShapeTable.
    /// </summary>
    public interface IShapeTableLocator : IUnitOfWorkDependency
    {
        ShapeTable Lookup(string themeName);
    }

    public class ShapeTableLocator : IShapeTableLocator
    {
        private readonly IShapeTableManager _shapeTableManager;
        readonly ConcurrentDictionary<string, ShapeTable> _shapeTables = new ConcurrentDictionary<string, ShapeTable>();

        public ShapeTableLocator(IShapeTableManager shapeTableManager)
        {
            _shapeTableManager = shapeTableManager;
        }

        public ShapeTable Lookup(string themeName)
        {
            return _shapeTables.GetOrAdd(themeName ?? "", _ => _shapeTableManager.GetShapeTable(themeName));
        }
    }
}