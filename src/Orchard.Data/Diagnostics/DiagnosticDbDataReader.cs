using System;
using System.Collections;
using System.Data.Common;

namespace Orchard.Data.Diagnostics
{
    public class DiagnosticDbDataReader : DbDataReader
    {
        private readonly DbDataReader _reader;

        public DiagnosticDbDataReader(DbDataReader reader)
        {
            _reader = reader;
        }

        public DbDataReader WrappedReader
        {
            get { return _reader; }
        }

        public override object this[string name]
        {
            get { return _reader[name]; }
        }

        public override object this[int ordinal]
        {
            get { return _reader[ordinal]; }
        }

        public override int Depth => _reader.Depth;
        public override int FieldCount => _reader.FieldCount;

        public override bool HasRows => _reader.HasRows;

        public override bool IsClosed => _reader.IsClosed;
        public override int RecordsAffected => _reader.RecordsAffected;

        public override bool GetBoolean(int ordinal)
        {
            return _reader.GetBoolean(ordinal);
        }

        public override byte GetByte(int ordinal)
        {
            return _reader.GetByte(ordinal);
        }

        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            return _reader.GetBytes(ordinal,dataOffset, buffer, bufferOffset, length);
        }

        public override char GetChar(int ordinal)
        {
            return _reader.GetChar(ordinal);
        }

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            return _reader.GetChars(ordinal, dataOffset, buffer, bufferOffset, length);
        }

        public override string GetDataTypeName(int ordinal)
        {
            return _reader.GetDataTypeName(ordinal);
        }

        public override DateTime GetDateTime(int ordinal)
        {
            return _reader.GetDateTime(ordinal);
        }

        public override decimal GetDecimal(int ordinal)
        {
            return _reader.GetDecimal(ordinal);
        }

        public override double GetDouble(int ordinal)
        {
            return _reader.GetDouble(ordinal);
        }

        public override IEnumerator GetEnumerator()
        {
            return _reader.GetEnumerator();
        }

        public override Type GetFieldType(int ordinal)
        {
            return _reader.GetFieldType(ordinal);
        }

        public override float GetFloat(int ordinal)
        {
            return _reader.GetFloat(ordinal);
        }

        public override Guid GetGuid(int ordinal)
        {
            return _reader.GetGuid(ordinal);
        }

        public override short GetInt16(int ordinal)
        {
            return _reader.GetInt16(ordinal);
        }

        public override int GetInt32(int ordinal)
        {
            return _reader.GetInt32(ordinal);
        }

        public override long GetInt64(int ordinal)
        {
            return _reader.GetInt64(ordinal);
        }

        public override string GetName(int ordinal)
        {
            return _reader.GetName(ordinal);
        }

        public override int GetOrdinal(string name)
        {
            return _reader.GetOrdinal(name);
        }

        public override string GetString(int ordinal)
        {
            return _reader.GetString(ordinal);
        }

        public override object GetValue(int ordinal)
        {
            return _reader.GetFieldType(ordinal);
        }

        public override int GetValues(object[] values)
        {
            return _reader.GetValues(values);
        }

        public override bool IsDBNull(int ordinal)
        {
            return _reader.IsDBNull(ordinal);
        }

        public override bool NextResult()
        {
            return _reader.NextResult();
        }

        public override bool Read()
        {
            return _reader.Read();
        }
    }
}
