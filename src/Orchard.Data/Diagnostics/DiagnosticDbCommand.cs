using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;

namespace Orchard.Data.Diagnostics
{
    public class DiagnosticDbCommand : DbCommand
    {
        private readonly DiagnosticSource _diagnostics;
        private readonly DbCommand _command;

        public DiagnosticDbCommand(DiagnosticSource diagnostics, DbCommand command)
        {
            if (diagnostics == null) throw new ArgumentNullException(nameof(diagnostics));
            if (command == null) throw new ArgumentNullException(nameof(command));

            _diagnostics = diagnostics;
            _command = command;
        }

        public override string CommandText
        {
            get { return _command.CommandText; }
            set { _command.CommandText = value; }
        }

        public override int CommandTimeout
        {
            get { return _command.CommandTimeout; }
            set { _command.CommandTimeout = value; }
        }

        public override CommandType CommandType
        {
            get { return _command.CommandType; }
            set { _command.CommandType = value; }
        }

        public override bool DesignTimeVisible
        {
            get { return _command.DesignTimeVisible; }
            set { _command.DesignTimeVisible = value; }
        }

        public override UpdateRowSource UpdatedRowSource
        {
            get { return _command.UpdatedRowSource; }
            set { _command.UpdatedRowSource = value; }
        }

        protected override DbConnection DbConnection
        {
            get { return _command.Connection; }
            set { _command.Connection = value; }
        }

        protected override DbParameterCollection DbParameterCollection
        {
            get { return _command.Parameters; }
        }

        protected override DbTransaction DbTransaction
        {
            get { return _command.Transaction; }
            set { _command.Transaction = value; }
        }

        public override void Cancel()
        {
            _command.Cancel();
        }

        public override int ExecuteNonQuery()
        {
            var startTimestamp = Stopwatch.GetTimestamp();
            var instanceId = Guid.NewGuid();

            _diagnostics
                .WriteCommandBefore(
                    this,
                    nameof(ExecuteNonQuery),
                    instanceId,
                    startTimestamp,
                    async: false);

            try
            {
                int value = _command.ExecuteNonQuery();

                var currentTimestamp = Stopwatch.GetTimestamp();

                _diagnostics
                    .WriteCommandAfter(
                        this,
                        nameof(ExecuteNonQuery),
                        instanceId,
                        startTimestamp,
                        currentTimestamp);

                return value;
            }
            catch (Exception ex)
            {
                var currentTimestamp = Stopwatch.GetTimestamp();

                _diagnostics
                    .WriteCommandError(
                        this,
                        nameof(ExecuteNonQuery),
                        instanceId,
                        startTimestamp,
                        currentTimestamp,
                        ex,
                        async: false);

                throw;
            }
        }

        public override object ExecuteScalar()
        {
            return _command.ExecuteScalar();
        }

        public override void Prepare()
        {
            _command.Prepare();
        }

        protected override DbParameter CreateDbParameter()
        {
            return _command.CreateParameter();
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            return _command.ExecuteReader(behavior);
        }
    }
}
