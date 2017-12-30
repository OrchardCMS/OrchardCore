using System;
using System.Collections.Generic;
using OrchardCore.Environment.Shell.Models;

namespace OrchardCore.Environment.Shell
{
    /// <summary>
    /// Represents the minimum set of fields stored for each tenant. This
    /// model is obtained from the IShellSettingsManager, which by default reads this
    /// from the App_Data Settings.txt files.
    /// </summary>
    public class ShellSettings
    {
        private TenantStatus _tenantStatus;
        private readonly IDictionary<string, string> _values;

        public ShellSettings() : this(new Dictionary<string, string>()) { }

        public ShellSettings(IDictionary<string, string> configuration)
        {
            _values = new Dictionary<string, string>(configuration);

            if (!configuration.ContainsKey(nameof(Status)) || !Enum.TryParse(configuration[nameof(Status)], true, out _tenantStatus))
            {
                _tenantStatus = TenantStatus.Invalid;
            }
        }

        /// <summary>
        /// Gets all keys held by this shell settings.
        /// </summary>
        public string this[string key]
        {
            get
            {
                string retVal;
                return _values.TryGetValue(key, out retVal) ? retVal : null;
            }
            set { _values[key] = value; }
        }

        public IDictionary<string, string> Configuration => _values;

        /// <summary>
        /// The name of the tenant.
        /// </summary>
        public string Name
        {
            get => this[nameof(Name)] ?? "";
            set => this[nameof(Name)] = value;
        }

        /// <summary>
        /// The host name of the tenant.
        /// </summary>
        public string RequestUrlHost
        {
            get => this[nameof(RequestUrlHost)];
            set => this[nameof(RequestUrlHost)] = value;
        }

        /// <summary>
        /// The request url prefix of the tenant.
        /// </summary>
        public string RequestUrlPrefix
        {
            get => this[nameof(RequestUrlPrefix)];
            set => _values[nameof(RequestUrlPrefix)] = value;
        }

        /// <summary>
        /// The database provider for the tenant.
        /// </summary>
        public string DatabaseProvider
        {
            get => this[nameof(DatabaseProvider)];
            set => _values[nameof(DatabaseProvider)] = value;
        }

        /// <summary>
        /// The data table prefix added to table names for this tenant.
        /// </summary>
        public string TablePrefix
        {
            get => this[nameof(TablePrefix)];
            set => _values[nameof(TablePrefix)] = value;
        }

        /// <summary>
        /// The database connection string.
        /// </summary>
        public string ConnectionString
        {
            get => this[nameof(ConnectionString)];
            set => _values[nameof(ConnectionString)] = value;
        }

        /// <summary>
        /// The encryption algorithm used for encryption services.
        /// </summary>
        public string EncryptionAlgorithm
        {
            get => this[nameof(EncryptionAlgorithm)];
            set => this[nameof(EncryptionAlgorithm)] = value;
        }

        /// <summary>
        /// The encryption key used for encryption services.
        /// </summary>
        public string EncryptionKey
        {
            get => this[nameof(EncryptionKey)];
            set => _values[nameof(EncryptionKey)] = value;
        }

        /// <summary>
        /// The hash algorithm used for encryption services.
        /// </summary>
        public string HashAlgorithm
        {
            get => this[nameof(HashAlgorithm)];
            set => this[nameof(HashAlgorithm)] = value;
        }

        /// <summary>
        /// The hash key used for encryption services.
        /// </summary>
        public string HashKey
        {
            get => this[nameof(HashKey)];
            set => this[nameof(HashKey)] = value;
        }

        public TenantStatus Status
        {
            get => _tenantStatus;
            set
            {
                _tenantStatus = value;
                this[nameof(Status)] = value.ToString();
            }
        }
    }
}
