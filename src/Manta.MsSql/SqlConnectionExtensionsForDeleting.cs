using System;
using System.Data;
using System.Data.SqlClient;

namespace Manta.MsSql
{
    internal static class SqlConnectionExtensionsForDeleting
    {
        private const string paramStreamName = "@StreamName";
        private const string paramExpectedVersion = "@ExpectedVersion";
        private const string paramToVersion = "@ToVersion";
        private const string paramToCreationDate = "@ToCreationDate";

        private const string mantaHardDeleteStreamCommand = "mantaHardDeleteStream";
        private const string mantaTruncateStreamToVersionCommand = "mantaTruncateStreamToVersion";
        private const string mantaTruncateStreamToCreationDateCommand = "mantaTruncateStreamToCreationDate";

        public static SqlCommand CreateCommandForHardDeletingStream(this SqlConnection cnn, string name, int expectedVersion)
        {
            var cmd = cnn.CreateCommand();
            cmd.CommandText = mantaHardDeleteStreamCommand;
            cmd.CommandType = CommandType.StoredProcedure;

            var p = cmd.Parameters.Add(paramStreamName, SqlDbType.VarChar, SqlClientExtensions.DefaultStreamNameLength);
            p.Value = name;
            p = cmd.Parameters.Add(paramExpectedVersion, SqlDbType.Int);
            p.Value = expectedVersion;

            return cmd;
        }

        public static SqlCommand CreateCommandForTruncateStreamToVersion(this SqlConnection cnn, string name, int expectedVersion, int toVersion)
        {
            var cmd = cnn.CreateCommand();
            cmd.CommandText = mantaTruncateStreamToVersionCommand;
            cmd.CommandType = CommandType.StoredProcedure;

            var p = cmd.Parameters.Add(paramStreamName, SqlDbType.VarChar, SqlClientExtensions.DefaultStreamNameLength);
            p.Value = name;
            p = cmd.Parameters.Add(paramExpectedVersion, SqlDbType.Int);
            p.Value = expectedVersion;
            p = cmd.Parameters.Add(paramToVersion, SqlDbType.Int);
            p.Value = toVersion;

            return cmd;
        }

        public static SqlCommand CreateCommandForTruncateStreamToCreationDate(this SqlConnection cnn, string name, int expectedVersion, DateTime toCreationDate)
        {
            var cmd = cnn.CreateCommand();
            cmd.CommandText = mantaTruncateStreamToCreationDateCommand;
            cmd.CommandType = CommandType.StoredProcedure;

            var p = cmd.Parameters.Add(paramStreamName, SqlDbType.VarChar, SqlClientExtensions.DefaultStreamNameLength);
            p.Value = name;
            p = cmd.Parameters.Add(paramExpectedVersion, SqlDbType.Int);
            p.Value = expectedVersion;
            p = cmd.Parameters.Add(paramToCreationDate, SqlDbType.DateTime2, 3);
            p.Value = toCreationDate;

            return cmd;
        }
    }
}
