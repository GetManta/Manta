using System.Data;
using System.Data.SqlClient;

namespace Manta.MsSql
{
    internal static class SqlConnectionExtensionsForAppending
    {
        private const string paramStreamName = "@StreamName";
        private const string paramCorrelationId = "@CorrelationId";
        private const string paramContractName = "@ContractName";
        private const string paramMessageVersion = "@MessageVersion";
        private const string paramMessageId = "@MessageId";
        private const string paramPayload = "@Payload";
        private const string paramMetadataPayload = "@MetadataPayload";

        private const string mantaAppendAnyVersionCommand = "mantaAppendAnyVersion";
        private const string mantaAppendExpectedVersionCommand = "mantaAppendExpectedVersion";
        private const string mantaAppendNoStreamCommand = "mantaAppendNoStream";

        public static SqlCommand CreateCommandToAppendingWithAnyVersion(this SqlConnection cnn, string name, UncommittedMessages data, MessageRecord msg)
        {
            return cnn
                .CreateCommand(mantaAppendAnyVersionCommand)
                .AddInputParam(paramStreamName, SqlDbType.VarChar, name, SqlClientExtensions.DefaultStreamNameLength)
                .AddInputParam(paramCorrelationId, SqlDbType.UniqueIdentifier, data.CorrelationId)
                .AddInputParam(paramContractName, SqlDbType.VarChar, msg.ContractName, SqlClientExtensions.DefaultContractNameLength)
                .AddInputParam(paramMessageId, SqlDbType.UniqueIdentifier, msg.MessageId)
                .AddInputParam(paramPayload, SqlDbType.VarBinary, msg.Payload)
                .AddInputParam(paramMetadataPayload, SqlDbType.VarBinary, data.Metadata);
        }

        public static SqlCommand CreateCommandToAppendingWithExpectedVersion(this SqlConnection cnn, string name, UncommittedMessages data, MessageRecord msg, int messageVersion)
        {
            return cnn
                .CreateCommand(mantaAppendExpectedVersionCommand)
                .AddInputParam(paramStreamName, SqlDbType.VarChar, name, SqlClientExtensions.DefaultStreamNameLength)
                .AddInputParam(paramCorrelationId, SqlDbType.UniqueIdentifier, data.CorrelationId)
                .AddInputParam(paramContractName, SqlDbType.VarChar, msg.ContractName, SqlClientExtensions.DefaultContractNameLength)
                .AddInputParam(paramMessageId, SqlDbType.UniqueIdentifier, msg.MessageId)
                .AddInputParam(paramMessageVersion, SqlDbType.Int, messageVersion)
                .AddInputParam(paramPayload, SqlDbType.VarBinary, msg.Payload)
                .AddInputParam(paramMetadataPayload, SqlDbType.VarBinary, data.Metadata);
        }

        public static SqlCommand CreateCommandToAppendingWithNoStream(this SqlConnection cnn, string name, UncommittedMessages data, MessageRecord msg)
        {
            return cnn
                .CreateCommand(mantaAppendNoStreamCommand)
                .AddInputParam(paramStreamName, SqlDbType.VarChar, name, SqlClientExtensions.DefaultStreamNameLength)
                .AddInputParam(paramCorrelationId, SqlDbType.UniqueIdentifier, data.CorrelationId)
                .AddInputParam(paramContractName, SqlDbType.VarChar, msg.ContractName, SqlClientExtensions.DefaultContractNameLength)
                .AddInputParam(paramMessageId, SqlDbType.UniqueIdentifier, msg.MessageId)
                .AddInputParam(paramPayload, SqlDbType.VarBinary, msg.Payload)
                .AddInputParam(paramMetadataPayload, SqlDbType.VarBinary, data.Metadata);
        }
    }
}
