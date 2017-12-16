using System.Data;
using System.Data.SqlClient;

namespace Manta.MsSql
{
    internal static class SqlConnectionExtensionsForDeleting
    {
        private const short defaultStreamNameLength = 512;

        private const string paramStreamName = "@StreamName";
        private const string paramExpectedVersion = "@ExpectedVersion";

        private const string mantaHardDeleteStreamCommand = "mantaHardDeleteStream";

        public static SqlCommand CreateCommandForHardDeletingStream(this SqlConnection cnn, string name, int expectedVersion)
        {
            var cmd = cnn.CreateCommand();
            cmd.CommandText = mantaHardDeleteStreamCommand;
            cmd.CommandType = CommandType.StoredProcedure;

            var p = cmd.Parameters.Add(paramStreamName, SqlDbType.VarChar, defaultStreamNameLength);
            p.Value = name;
            p = cmd.Parameters.Add(paramExpectedVersion, SqlDbType.Int);
            p.Value = expectedVersion;

            return cmd;
        }
    }
}
