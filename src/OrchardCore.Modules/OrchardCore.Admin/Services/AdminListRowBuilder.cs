using System.Collections;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;

namespace OrchardCore.Admin.Services;

internal sealed class AdminListRowBuilder
{
    private readonly IServiceProvider _serviceProvider;

    public AdminListRowBuilder(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task<IList<IShape>> BuildAsync(IEnumerable source, Type itemType, string displayType, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        cancellationToken.ThrowIfCancellationRequested();

        if (itemType == null)
        {
            var types = source.GetType().GetInterfaces().Append(source.GetType())
                .Where(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                .Select(type => type.GenericTypeArguments[0])
                .Distinct()
                .ToArray();

            if (types.Length != 1)
            {
                throw new ArgumentException("Admin list items must implement an unambiguous IEnumerable<T>, or specify an item type.", nameof(source));
            }

            itemType = types[0];
        }

        var builder = (IRowBuilder)ActivatorUtilities.CreateInstance(_serviceProvider, typeof(RowBuilder<>).MakeGenericType(itemType));
        return builder.BuildAsync(source, displayType, cancellationToken);
    }

    private interface IRowBuilder
    {
        Task<IList<IShape>> BuildAsync(IEnumerable source, string displayType, CancellationToken cancellationToken);
    }

    private sealed class RowBuilder<TModel> : IRowBuilder
    {
        private readonly IAdminListItemDisplayManager<TModel> _displayManager;
        private readonly IUpdateModelAccessor _updateModelAccessor;

        public RowBuilder(IAdminListItemDisplayManager<TModel> displayManager, IUpdateModelAccessor updateModelAccessor)
        {
            _displayManager = displayManager;
            _updateModelAccessor = updateModelAccessor;
        }

        public async Task<IList<IShape>> BuildAsync(IEnumerable source, string displayType, CancellationToken cancellationToken)
        {
            var rows = new List<IShape>();
            foreach (var item in source)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (item is not TModel model)
                {
                    throw new ArgumentException($"Every admin list item must be a non-null instance of {typeof(TModel).FullName}.", nameof(source));
                }

                rows.Add(await _displayManager.BuildDisplayAsync(model, _updateModelAccessor.ModelUpdater, displayType));
            }

            return rows;
        }
    }
}
