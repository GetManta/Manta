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
        private const string paramStreamName = "@StreamName";
        private const string paramFromVersion = "@FromVersion";
        private const string paramMessageVersion = "@MessageVersion";

        private const string mantaReadStreamForwardCommand = "mantaReadStreamForward";
        private const string mantaReadHeadMessagePosition = "mantaReadHeadMessagePosition";
        private const string mantaReadMessageCommand = "mantaReadMessage";


        public static SqlCommand CreateCommandForReadStreamForward(this SqlConnection cnn, string name, int fromVersion)
        {
            return cnn
                .CreateCommand(mantaReadStreamForwardCommand)
                .AddInputParam(paramStreamName, SqlDbType.VarChar, name, SqlClientExtensions.DefaultStreamNameLength)
                .AddInputParam(paramFromVersion, SqlDbType.Int, fromVersion);
        }

        public static SqlCommand CreateCommandForReadHeadMessagePosition(this SqlConnection cnn)
        {
            return cnn.CreateCommand(mantaReadHeadMessagePosition);
        }

        public static SqlCommand CreateCommandForReadMessage(this SqlConnection cnn, string name, int messageVersion)
        {
            return cnn
                .CreateCommand(mantaReadMessageCommand)
                .AddInputParam(paramStreamName, SqlDbType.VarChar, name, SqlClientExtensions.DefaultStreamNameLength)
                .AddInputParam(paramMessageVersion, SqlDbType.Int, messageVersion);
        }

        private const byte columnIndexForMessageId = 0;
        private const byte columnIndexForMessageVersion = 1;
        private const byte columnIndexForContractName = 2;
        private const byte columnIndexForPayload = 3;

        public static async Task<RecordedMessage> GetRecordedMessage(this SqlDataReader reader, CancellationToken cancellationToken)
        {
            return new RecordedMessage(
                await reader.GetFieldValueAsync<Guid>(columnIndexForMessageId, cancellationToken).NotOnCapturedContext(),
                await reader.GetFieldValueAsync<int>(columnIndexForMessageVersion, cancellationToken).NotOnCapturedContext(),
                await reader.GetFieldValueAsync<string>(columnIndexForContractName, cancellationToken).NotOnCapturedContext(),
                await reader.GetFieldValueAsync<byte[]>(columnIndexForPayload, cancellationToken).NotOnCapturedContext());
        }
    }
}