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
            return cnn
                .CreateCommand(mantaHardDeleteStreamCommand)
                .AddInputParam(paramStreamName, SqlDbType.VarChar, name, SqlClientExtensions.DefaultStreamNameLength)
                .AddInputParam(paramExpectedVersion, SqlDbType.Int, expectedVersion);
        }

        public static SqlCommand CreateCommandForTruncateStreamToVersion(this SqlConnection cnn, string name, int expectedVersion, int toVersion)
        {
            return cnn
                .CreateCommand(mantaTruncateStreamToVersionCommand)
                .AddInputParam(paramStreamName, SqlDbType.VarChar, name, SqlClientExtensions.DefaultStreamNameLength)
                .AddInputParam(paramExpectedVersion, SqlDbType.Int, expectedVersion)
                .AddInputParam(paramToVersion, SqlDbType.Int, toVersion);
        }

        public static SqlCommand CreateCommandForTruncateStreamToCreationDate(this SqlConnection cnn, string name, int expectedVersion, DateTime toCreationDate)
        {
            return cnn
                .CreateCommand(mantaTruncateStreamToCreationDateCommand)
                .AddInputParam(paramStreamName, SqlDbType.VarChar, name, SqlClientExtensions.DefaultStreamNameLength)
                .AddInputParam(paramExpectedVersion, SqlDbType.Int, expectedVersion)
                .AddInputParam(paramToCreationDate, SqlDbType.DateTime2, toCreationDate, 3);
        }
    }
}
