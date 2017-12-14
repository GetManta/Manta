using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Manta.Sceleton;

namespace Manta.MsSql
{
    internal static class SqlConnectionExtensionsForReading
    {
        private const short defaultStreamNameLength = 512;

        private const string paramStreamName = "@StreamName";
        private const string paramFromVersion = "@FromVersion";
        private const string paramMessageVersion = "@MessageVersion";

        private const string mantaReadStreamForwardCommand = "mantaReadStreamForward";
        private const string mantaReadHeadMessagePosition = "mantaReadHeadMessagePosition";
        private const string mantaReadMessageCommand = "mantaReadMessage";


        public static SqlCommand CreateCommandForReadStreamForward(this SqlConnection cnn, string name, int fromVersion)
        {
            var cmd = cnn.CreateCommand();
            cmd.CommandText = mantaReadStreamForwardCommand;
            cmd.CommandType = CommandType.StoredProcedure;

            var p = cmd.Parameters.Add(paramStreamName, SqlDbType.VarChar, defaultStreamNameLength);
            p.Value = name;
            p = cmd.Parameters.Add(paramFromVersion, SqlDbType.Int);
            p.Value = fromVersion;

            return cmd;
        }

        public static SqlCommand CreateCommandForReadHeadMessagePosition(this SqlConnection cnn)
        {
            var cmd = cnn.CreateCommand();
            cmd.CommandText = mantaReadHeadMessagePosition;
            cmd.CommandType = CommandType.StoredProcedure;
            return cmd;
        }

        public static SqlCommand CreateCommandForReadMessage(this SqlConnection cnn, string name, int messageVersion)
        {
            var cmd = cnn.CreateCommand();
            cmd.CommandText = mantaReadMessageCommand;
            cmd.CommandType = CommandType.StoredProcedure;

            var p = cmd.Parameters.Add(paramStreamName, SqlDbType.VarChar, defaultStreamNameLength);
            p.Value = name;
            p = cmd.Parameters.Add(paramMessageVersion, SqlDbType.Int);
            p.Value = messageVersion;

            return cmd;
        }

        private const byte columnIndexForMessageId = 0;
        private const byte columnIndexForMessageVersion = 1;
        private const byte columnIndexForContractId = 2;
        private const byte columnIndexForPayload = 3;

        public static async Task<RecordedMessage> GetRecordedMessage(this SqlDataReader reader, CancellationToken cancellationToken)
        {
            return new RecordedMessage(
                await reader.GetFieldValueAsync<Guid>(columnIndexForMessageId, cancellationToken).NotOnCapturedContext(),
                await reader.GetFieldValueAsync<int>(columnIndexForMessageVersion, cancellationToken).NotOnCapturedContext(),
                await reader.GetFieldValueAsync<int>(columnIndexForContractId, cancellationToken).NotOnCapturedContext(),
                await reader.GetFieldValueAsync<byte[]>(columnIndexForPayload, cancellationToken).NotOnCapturedContext());
        }
    }
}
