using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace StackExchange.Profiling.Data
{
    /// <summary>
    /// Wraps a database connection, allowing SQL execution timings to be collected when a <see cref="MiniProfiler"/> session is started.
    /// This class is copied from Dapper as the original ProfileDbConnection stores a copy of MiniProfiler.Current
    /// and for Sqlite we reuse the connection overtime, which means the MiniProfiler instance is obsolete.
    /// </summary>
    [System.ComponentModel.DesignerCategory("")]
    public class ContextProfiledDbConnection : DbConnection
    {
        private DbConnection _connection;
        private IAsyncProfilerProvider _profilerProvider;

        /// <summary>
        /// Gets the current profiler instance; could be null.
        /// </summary>
        public IDbProfiler Profiler => _profilerProvider.CurrentProfiler;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfiledDbConnection"/> class. 
        /// Returns a new <see cref="ProfiledDbConnection"/> that wraps <paramref name="connection"/>, 
        /// providing query execution profiling. If profiler is null, no profiling will occur.
        /// </summary>
        /// <param name="connection"><c>Your provider-specific flavour of connection, e.g. SqlConnection, OracleConnection</c></param>
        /// <param name="profiler">The currently started <see cref="MiniProfiler"/> or null.</param>
        /// <exception cref="ArgumentNullException">Throws when <paramref name="connection"/> is <c>null</c>.</exception>
        public ContextProfiledDbConnection(DbConnection connection, IAsyncProfilerProvider profilerProvider)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _connection.StateChange += StateChangeHandler;
            _profilerProvider = profilerProvider;
        }

        /// <summary>
        /// Gets the connection that this ProfiledDbConnection wraps.
        /// </summary>
        public DbConnection WrappedConnection => _connection;

        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        public override string ConnectionString
        {
            get => _connection.ConnectionString;
            set => _connection.ConnectionString = value;
        }

        /// <summary>
        /// Gets the time to wait while establishing a connection before terminating the attempt and generating an error.
        /// </summary>
        public override int ConnectionTimeout => _connection.ConnectionTimeout;

        /// <summary>
        /// Gets the name of the current database after a connection is opened, 
        /// or the database name specified in the connection string before the connection is opened.
        /// </summary>
        public override string Database => _connection.Database;

        /// <summary>
        /// Gets the name of the database server to which to connect.
        /// </summary>
        public override string DataSource => _connection.DataSource;

        /// <summary>
        /// Gets a string that represents the version of the server to which the object is connected.
        /// </summary>
        public override string ServerVersion => _connection.ServerVersion;

        /// <summary>
        /// Gets the current state of the connection.
        /// </summary>
        public override ConnectionState State => _connection.State;

        /// <summary>
        /// Changes the current database for an open connection.
        /// </summary>
        /// <param name="databaseName">The new database name.</param>
        public override void ChangeDatabase(string databaseName) => _connection.ChangeDatabase(databaseName);

        /// <summary>
        /// Closes the connection to the database.
        /// This is the preferred method of closing any open connection.
        /// </summary>
        public override void Close() => _connection.Close();

        /// <summary>
        /// Opens a database connection with the settings specified by the <see cref="ConnectionString"/>.
        /// </summary>
        public override void Open()
        {
            var miniProfiler = _profilerProvider.CurrentProfiler as MiniProfiler;
            if (miniProfiler == null || /*!miniProfiler.IsActive ||*/ miniProfiler.Options?.TrackConnectionOpenClose == false)
            {
                _connection.Open();
                return;
            }

            using (miniProfiler.CustomTiming("sql", "Connection Open()", nameof(Open)))
            {
                _connection.Open();
            }
        }

        /// <summary>
        /// Asynchronously opens a database connection with the settings specified by the <see cref="ConnectionString"/>.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for this async operation.</param>
        public override async Task OpenAsync(CancellationToken cancellationToken)
        {
            var miniProfiler = _profilerProvider.CurrentProfiler as MiniProfiler;
            if (miniProfiler == null || /*!miniProfiler.IsActive ||*/ miniProfiler.Options?.TrackConnectionOpenClose == false)
            {
                await _connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                return;
            }

            using (miniProfiler.CustomTiming("sql", "Connection OpenAsync()", nameof(OpenAsync)))
            {
                await _connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Starts a database transaction.
        /// </summary>
        /// <param name="isolationLevel">Specifies the isolation level for the transaction.</param>
        /// <returns>An object representing the new transaction.</returns>
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return new ProfiledDbTransaction(_connection.BeginTransaction(isolationLevel), new ProfiledDbConnection(_connection, _profilerProvider.CurrentProfiler));
        }

        /// <summary>
        /// Creates and returns a <see cref="DbCommand"/> object associated with the current connection.
        /// </summary>
        /// <returns>A <see cref="ProfiledDbCommand"/> wrapping the created <see cref="DbCommand"/>.</returns>
        protected override DbCommand CreateDbCommand() => new ProfiledDbCommand(_connection.CreateCommand(), this, _profilerProvider.CurrentProfiler);

        /// <summary>
        /// Dispose the underlying connection.
        /// </summary>
        /// <param name="disposing">false if preempted from a <c>finalizer</c></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && _connection != null)
            {
                _connection.StateChange -= StateChangeHandler;
                _connection.Dispose();
            }
            base.Dispose(disposing);
            _connection = null;
            _profilerProvider = null;
        }

        /// <summary>
        /// The state change handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="stateChangeEventArguments">The state change event arguments.</param>
        private void StateChangeHandler(object sender, StateChangeEventArgs stateChangeEventArguments)
        {
            OnStateChange(stateChangeEventArguments);
        }

#if !NETSTANDARD1_5
        /// <summary>
        /// Gets a value indicating whether events can be raised.
        /// </summary>
        protected override bool CanRaiseEvents => true;

        /// <summary>
        /// Enlist the transaction.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        public override void EnlistTransaction(System.Transactions.Transaction transaction) => _connection.EnlistTransaction(transaction);

        /// <summary>
        /// Gets the database schema.
        /// </summary>
        /// <returns>The <see cref="DataTable"/>.</returns>
        public override DataTable GetSchema() => _connection.GetSchema();

        /// <summary>
        /// Gets the collection schema.
        /// </summary>
        /// <param name="collectionName">The collection name.</param>
        /// <returns>The <see cref="DataTable"/>.</returns>
        public override DataTable GetSchema(string collectionName) => _connection.GetSchema(collectionName);

        /// <summary>
        /// Gets the filtered collection schema.
        /// </summary>
        /// <param name="collectionName">The collection name.</param>
        /// <param name="restrictionValues">The restriction values.</param>
        /// <returns>The <see cref="DataTable"/>.</returns>
        public override DataTable GetSchema(string collectionName, string[] restrictionValues) => _connection.GetSchema(collectionName, restrictionValues);
#endif
    }
}