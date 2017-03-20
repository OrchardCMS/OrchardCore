using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.Tenant.Descriptor;
using OrchardCore.Tenant.Descriptor.Models;
using Orchard.Events;
using YesSql.Core.Services;

namespace OrchardCore.Tenant.Data.Descriptors
{
    /// <summary>
    /// Implements <see cref="ITenantDescriptorManager"/> by providing the list of features store in the database.
    /// </summary>
    public class TenantDescriptorManager : ITenantDescriptorManager
    {
        private readonly TenantSettings _tenantSettings;
        private readonly IEventBus _eventBus;
        private readonly ISession _session;
        private readonly ILogger _logger;
        private TenantDescriptor _tenantDescriptor;

        public TenantDescriptorManager(
            TenantSettings tenantSettings,
            IEventBus eventBus,
            ISession session,
            ILogger<TenantDescriptorManager> logger)
        {
            _tenantSettings = tenantSettings;
            _eventBus = eventBus;
            _session = session;
            _logger = logger;
        }

        public async Task<TenantDescriptor> GetTenantDescriptorAsync()
        {
            // Prevent multiple queries during the same request
            if (_tenantDescriptor == null)
            {
                _tenantDescriptor = await _session.QueryAsync<TenantDescriptor>().FirstOrDefault();
            }

            return _tenantDescriptor;
        }

        public async Task UpdateTenantDescriptorAsync(int priorSerialNumber, IEnumerable<TenantFeature> enabledFeatures, IEnumerable<TenantParameter> parameters)
        {
            var tenantDescriptorRecord = await GetTenantDescriptorAsync();
            var serialNumber = tenantDescriptorRecord == null ? 0 : tenantDescriptorRecord.SerialNumber;
            if (priorSerialNumber != serialNumber)
            {
                throw new InvalidOperationException("Invalid serial number for tenant descriptor");
            }

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Updating tenant descriptor for tenant '{0}'...", _tenantSettings.Name);
            }

            if (tenantDescriptorRecord == null)
            {
                tenantDescriptorRecord = new TenantDescriptor { SerialNumber = 1 };
            }
            else
            {
                tenantDescriptorRecord.SerialNumber++;
            }

            tenantDescriptorRecord.Features = enabledFeatures.ToList();
            tenantDescriptorRecord.Parameters = parameters.ToList();

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Tenant descriptor updated for tenant '{0}'.", _tenantSettings.Name);
            }

            _session.Save(tenantDescriptorRecord);

            // Update cached reference
            _tenantDescriptor = tenantDescriptorRecord;

            await _eventBus.NotifyAsync<ITenantDescriptorManagerEventHandler>(e => e.Changed(tenantDescriptorRecord, _tenantSettings.Name));
        }
    }
}