using System;
using System.Data;
using System.Data.Common;
using StackExchange.Profiling.Data;

namespace OrchardCore.MiniProfiler
{
    internal class CurrentDbProfiler : IDbProfiler
    {
        private readonly Func<IDbProfiler> _profiler;

        public CurrentDbProfiler(Func<IDbProfiler> profiler) => _profiler = profiler;

        public bool IsActive => ((IDbProfiler)StackExchange.Profiling.MiniProfiler.Current)?.IsActive ?? false;

        public void ExecuteFinish(IDbCommand profiledDbCommand, SqlExecuteType executeType, DbDataReader reader)
            => _profiler()?.ExecuteFinish(profiledDbCommand, executeType, reader);

        public void ExecuteStart(IDbCommand profiledDbCommand, SqlExecuteType executeType)
            => _profiler()?.ExecuteStart(profiledDbCommand, executeType);

        public void OnError(IDbCommand profiledDbCommand, SqlExecuteType executeType, Exception exception)
            => _profiler()?.OnError(profiledDbCommand, executeType, exception);

        public void ReaderFinish(IDataReader reader) => _profiler()?.ReaderFinish(reader);
    }
}
