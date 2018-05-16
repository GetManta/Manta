using System;
using System.Data;
using System.Data.SqlClient;

namespace Manta.MsSql
{
    internal static class SqlClientExtensions
    {
        public const short DefaultStreamNameLength = 512;
        public const byte DefaultContractNameLength = 128;

        private const short duplicateKeyViolationErrorNumber = 2627;
        private const short duplicateUniqueIndexViolationErrorNumber = 2601;
        private const string wrongExpectedVersionKey = "WrongExpectedVersion";

        public static bool IsUniqueConstraintViolation(this SqlException e, string indexName = null)
        {
            return (e.Number == duplicateKeyViolationErrorNumber || e.Number == duplicateUniqueIndexViolationErrorNumber)
                && (indexName == null || e.Message.Contains($"'{indexName}'"));
        }

        public static bool IsWrongExpectedVersionRised(this SqlException e)
        {
            return e.Message.StartsWith(wrongExpectedVersionKey, StringComparison.OrdinalIgnoreCase);
        }

        public static SqlCommand CreateCommand(this SqlConnection cnn, string commandText, int? commandTimeout = null)
        {
            var cmd = cnn.CreateCommand();
            cmd.CommandText = commandText;
            cmd.CommandType = CommandType.StoredProcedure;
            if (commandTimeout != null) cmd.CommandTimeout = commandTimeout.Value;
            return cmd;
        }

        public static SqlCommand AddInputParam(this SqlCommand cmd, string name, SqlDbType type, object value, int? size = null)
        {
            var p = cmd.Parameters.Add(name, type);
            if (size != null) p.Size = size.Value;
            p.Value = value;
            return cmd;
        }

        public static SqlCommand AddInputParam(this SqlCommand cmd, string name, SqlDbType type, ArraySegment<byte> value)
        {
            var p = cmd.Parameters.Add(name, type);
            var buffer = new byte[value.Count];
            Array.Copy(value.Array, value.Offset, buffer, 0, value.Count);
            p.Value = buffer;
            return cmd;
        }
    }
}
