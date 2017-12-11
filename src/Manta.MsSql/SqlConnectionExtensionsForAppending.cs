using System.Data;
using System.Data.SqlClient;

namespace Manta.MsSql
{
    internal static class SqlConnectionExtensionsForAppending
    {
        private const short defaultStreamNameLength = 512;

        private const string paramStreamName = "@StreamName";
        private const string paramCorrelationId = "@CorrelationId";
        private const string paramContractId = "@ContractId";
        private const string paramMessageVersion = "@MessageVersion";
        private const string paramMessageId = "@MessageId";
        private const string paramPayload = "@Payload";

        private const string mantaAppendAnyVersionCommand = "mantaAppendAnyVersion";

        public static SqlCommand CreateCommandToAppendingWithAnyVersion(this SqlConnection cnn, string name, UncommittedMessages data, MessageRecord msg)
        {
            var cmd = cnn.CreateCommand();
            cmd.CommandText = mantaAppendAnyVersionCommand;
            cmd.CommandType = CommandType.StoredProcedure;

            var p = cmd.Parameters.Add(paramStreamName, SqlDbType.VarChar, defaultStreamNameLength);
            p.Value = name;
            p = cmd.Parameters.Add(paramCorrelationId, SqlDbType.UniqueIdentifier);
            p.Value = data.CorrelationId;
            p = cmd.Parameters.Add(paramContractId, SqlDbType.Int);
            p.Value = msg.ContractId;
            p = cmd.Parameters.Add(paramMessageId, SqlDbType.UniqueIdentifier);
            p.Value = msg.MessageId;
            p = cmd.Parameters.Add(paramPayload, SqlDbType.VarBinary);
            p.Value = msg.Payload;

            return cmd;
        }

        private const string mantaAppendExpectedVersionCommand = "mantaAppendExpectedVersion";

        public static SqlCommand CreateCommandToAppendingWithExpectedVersion(this SqlConnection cnn, string name, UncommittedMessages data, MessageRecord msg, int messageVersion)
        {
            var cmd = cnn.CreateCommand();
            cmd.CommandText = mantaAppendExpectedVersionCommand;
            cmd.CommandType = CommandType.StoredProcedure;

            var p = cmd.Parameters.Add(paramStreamName, SqlDbType.VarChar, defaultStreamNameLength);
            p.Value = name;
            p = cmd.Parameters.Add(paramCorrelationId, SqlDbType.UniqueIdentifier);
            p.Value = data.CorrelationId;
            p = cmd.Parameters.Add(paramContractId, SqlDbType.Int);
            p.Value = msg.ContractId;
            p = cmd.Parameters.Add(paramMessageId, SqlDbType.UniqueIdentifier);
            p.Value = msg.MessageId;
            p = cmd.Parameters.Add(paramMessageVersion, SqlDbType.Int);
            p.Value = messageVersion;
            p = cmd.Parameters.Add(paramPayload, SqlDbType.VarBinary);
            p.Value = msg.Payload;

            return cmd;
        }

        private const string mantaAppendNoStreamCommand = "mantaAppendNoStream";

        public static SqlCommand CreateCommandToAppendingWithNoStream(this SqlConnection cnn, string name, UncommittedMessages data, MessageRecord msg)
        {
            var cmd = cnn.CreateCommand();
            cmd.CommandText = mantaAppendNoStreamCommand;
            cmd.CommandType = CommandType.StoredProcedure;

            var p = cmd.Parameters.Add(paramStreamName, SqlDbType.VarChar, defaultStreamNameLength);
            p.Value = name;
            p = cmd.Parameters.Add(paramCorrelationId, SqlDbType.UniqueIdentifier);
            p.Value = data.CorrelationId;
            p = cmd.Parameters.Add(paramContractId, SqlDbType.Int);
            p.Value = msg.ContractId;
            p = cmd.Parameters.Add(paramMessageId, SqlDbType.UniqueIdentifier);
            p.Value = msg.MessageId;
            p = cmd.Parameters.Add(paramPayload, SqlDbType.VarBinary);
            p.Value = msg.Payload;

            return cmd;
        }
    }
}
