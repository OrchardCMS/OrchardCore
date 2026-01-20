using System;
using System.Data;
using System.Data.Common;
using StackExchange.Profiling.Data;

namespace OrchardCore.MiniProfiler
{
    internal class CurrentDbProfiler : IDbProfiler
    {
        private Func<IDbProfiler> GetProfiler { get; }
        public CurrentDbProfiler(Func<IDbProfiler> getProfiler) => GetProfiler = getProfiler;

        public bool IsActive => ((IDbProfiler)StackExchange.Profiling.MiniProfiler.Current)?.IsActive ?? false;

        public void ExecuteFinish(IDbCommand profiledDbCommand, SqlExecuteType executeType, DbDataReader reader) =>
            GetProfiler()?.ExecuteFinish(profiledDbCommand, executeType, reader);

        public void ExecuteStart(IDbCommand profiledDbCommand, SqlExecuteType executeType) =>
            GetProfiler()?.ExecuteStart(profiledDbCommand, executeType);

        public void OnError(IDbCommand profiledDbCommand, SqlExecuteType executeType, Exception exception) =>
            GetProfiler()?.OnError(profiledDbCommand, executeType, exception);

        public void ReaderFinish(IDataReader reader) =>
            GetProfiler()?.ReaderFinish(reader);
    }
}
