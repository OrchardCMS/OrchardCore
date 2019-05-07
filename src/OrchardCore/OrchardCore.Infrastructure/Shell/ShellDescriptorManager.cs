using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Data;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Environment.Shell.Descriptor;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Modules;
using YesSql;

namespace OrchardCore.Environment.Shell.Data.Descriptors
{
    /// <summary>
    /// Implements <see cref="IShellDescriptorManager"/> by providing the list of features store in the database. 
    /// </summary>
    public class ShellDescriptorManager : IShellDescriptorManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ShellSettings _shellSettings;
        private readonly IShellConfiguration _shellConfiguration;
        private readonly IEnumerable<ShellFeature> _alwaysEnabledFeatures;
        private readonly IEnumerable<IShellDescriptorManagerEventHandler> _shellDescriptorManagerEventHandlers;
        private readonly ISession _session;
        private readonly ILogger _logger;
        private ShellDescriptor _shellDescriptor;

        public ShellDescriptorManager(
            IServiceProvider serviceProvider,
            ShellSettings shellSettings,
            IShellConfiguration shellConfiguration,
            IEnumerable<ShellFeature> shellFeatures,
            IEnumerable<IShellDescriptorManagerEventHandler> shellDescriptorManagerEventHandlers,
            ISession session,
            ILogger<ShellDescriptorManager> logger)
        {
            _serviceProvider = serviceProvider;
            _shellSettings = shellSettings;
            _shellConfiguration = shellConfiguration;
            _alwaysEnabledFeatures = shellFeatures.Where(f => f.AlwaysEnabled).ToArray();
            _shellDescriptorManagerEventHandlers = shellDescriptorManagerEventHandlers;
            _session = session;
            _logger = logger;
        }

        public async Task<ShellDescriptor> GetShellDescriptorAsync()
        {
            // Prevent multiple queries during the same request
            if (_shellDescriptor == null)
            {
                _shellDescriptor = await _session.Query<ShellDescriptor>().FirstOrDefaultAsync();

                if (_shellDescriptor == null)
                {
                    // If no descriptor was found, try to update from Beta2
                    await UpgradeFromBeta2();
                    _shellDescriptor = await _session.Query<ShellDescriptor>().FirstOrDefaultAsync();
                }

                if (_shellDescriptor != null)
                {
                    var configuredFeatures = new ConfiguredFeatures();
                    _shellConfiguration.Bind(configuredFeatures);

                    var features = _alwaysEnabledFeatures.Concat(configuredFeatures.Features
                        .Select(id => new ShellFeature(id) { AlwaysEnabled = true })).Distinct();

                    _shellDescriptor.Features = features
                        .Concat(_shellDescriptor.Features)
                        .Distinct()
                        .ToList();
                }
            }

            return _shellDescriptor;
        }

        public async Task UpdateShellDescriptorAsync(int priorSerialNumber, IEnumerable<ShellFeature> enabledFeatures, IEnumerable<ShellParameter> parameters)
        {
            var shellDescriptorRecord = await GetShellDescriptorAsync();
            var serialNumber = shellDescriptorRecord == null
                ? 0
                : shellDescriptorRecord.SerialNumber;

            if (priorSerialNumber != serialNumber)
            {
                throw new InvalidOperationException("Invalid serial number for shell descriptor");
            }

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Updating shell descriptor for tenant '{TenantName}' ...", _shellSettings.Name);
            }

            if (shellDescriptorRecord == null)
            {
                shellDescriptorRecord = new ShellDescriptor { SerialNumber = 1 };
            }
            else
            {
                shellDescriptorRecord.SerialNumber++;
            }

            shellDescriptorRecord.Features = _alwaysEnabledFeatures.Concat(enabledFeatures).Distinct().ToList();
            shellDescriptorRecord.Parameters = parameters.ToList();

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Shell descriptor updated for tenant '{TenantName}'.", _shellSettings.Name);
            }

            _session.Save(shellDescriptorRecord);

            // Update cached reference
            _shellDescriptor = shellDescriptorRecord;

            await _shellDescriptorManagerEventHandlers.InvokeAsync(e => e.Changed(shellDescriptorRecord, _shellSettings.Name), _logger);
        }

        private class ConfiguredFeatures
        {
            public string[] Features { get; set; } = Array.Empty<string>();
        }

        private async Task UpgradeFromBeta2()
        {
            // TODO: Can be removed when going RC as users are not supposed to go from beta2 to RC
            // c.f. https://github.com/OrchardCMS/OrchardCore/issues/2439

            var connectionAccessor = _serviceProvider.GetRequiredService<IDbConnectionAccessor>();

            using (var connection = connectionAccessor.CreateConnection())
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction())
                {
                    var dialect = SqlDialectFactory.For(connection);
                    var tablePrefix = _shellSettings["TablePrefix"];

                    if (!String.IsNullOrEmpty(tablePrefix))
                    {
                        tablePrefix += '_';
                    }

                    var documentTable = dialect.QuoteForTableName($"{tablePrefix}{nameof(Document)}");

                    var oldShellDescriptorType = "OrchardCore.Environment.Shell.Descriptor.Models.ShellDescriptor, OrchardCore.Environment.Shell.Abstractions";
                    var newShellDescriptorType = "OrchardCore.Environment.Shell.Descriptor.Models.ShellDescriptor, OrchardCore.Abstractions";

                    var updateShellDescriptorCmd = $"UPDATE {documentTable} SET {dialect.QuoteForColumnName(nameof(Document.Type))} = {dialect.GetSqlValue(newShellDescriptorType)} WHERE {dialect.QuoteForColumnName(nameof(Document.Type))} = {dialect.GetSqlValue(oldShellDescriptorType)}";

                    var oldShellStateType = "OrchardCore.Environment.Shell.State.ShellState, OrchardCore.Environment.Shell.Abstractions";
                    var newShellStateType = "OrchardCore.Environment.Shell.State.ShellState, OrchardCore.Abstractions";

                    var updateShellStateCmd = $"UPDATE {documentTable} SET {dialect.QuoteForColumnName(nameof(Document.Type))} = {dialect.GetSqlValue(newShellStateType)} WHERE {dialect.QuoteForColumnName(nameof(Document.Type))} = {dialect.GetSqlValue(oldShellStateType)}";

                    await connection.ExecuteAsync(updateShellDescriptorCmd, null, transaction);
                    await connection.ExecuteAsync(updateShellStateCmd, null, transaction);

                    transaction.Commit();
                }
            }
        }
    }
}