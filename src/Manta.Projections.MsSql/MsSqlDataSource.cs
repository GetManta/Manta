using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Manta.Sceleton;

namespace Manta.Projections.MsSql
{
    public class MsSqlDataSource : IStreamDataSource
    {
        private readonly string _connectionString;

        public MsSqlDataSource(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int> Fetch(ITargetBlock<MessageRaw> buffer, long fromPosition, int limit, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var cnn = new SqlConnection(_connectionString))
            {
                var cmd = cnn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = spuReadAllStreamsForward;
                cmd.AddInputParam(limitParamName, SqlDbType.Int, limit);
                cmd.AddInputParam(fromPositionParamName, SqlDbType.Int, fromPosition);

                await cnn.OpenAsync(cancellationToken).NotOnCapturedContext();
                var rows = 0;
                using (var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleResult, cancellationToken).NotOnCapturedContext())
                {
                    while (await reader.ReadAsync(cancellationToken).NotOnCapturedContext())
                    {
                        var raw = new MessageRaw
                        {
                            StreamId = reader.GetString(colIndexForStreamName),
                            MessageContractName = reader.GetString(colIndexForContractName),
                            CorrelationId = reader.GetGuid(colIndexForCorrelationId),
                            Timestamp = reader.GetDateTime(colIndexForTimestamp),
                            MessageId = reader.GetGuid(colIndexForMessageId),
                            MessageVersion = reader.GetInt32(colIndexForMessageVersion),
                            MessagePosition = reader.GetInt64(colIndexForMessagePosition),
                            MessagePayload = reader.GetStream(colIndexForMessagePayload),
                            MessageMetadataPayload = reader.IsDBNull(colIndexForMessageMetadataPayload)
                                ? null
                                : reader.GetStream(colIndexForMessageMetadataPayload)
                        };

                        await buffer.SendAsync(raw, cancellationToken).NotOnCapturedContext();
                        rows++;
                    }
                }

                buffer.Complete();
                return rows;
            }
        }

        private const string spuReadAllStreamsForward = "mantaReadAllStreamsForward";
        private const string limitParamName = "@Limit";
        private const string fromPositionParamName = "@FromPosition";

        private const byte colIndexForStreamName = 0;
        private const byte colIndexForContractName = 1;
        private const byte colIndexForCorrelationId = 2;
        private const byte colIndexForTimestamp = 3;
        private const byte colIndexForMessageId = 4;
        private const byte colIndexForMessageVersion = 5;
        private const byte colIndexForMessagePosition = 6;
        private const byte colIndexForMessagePayload = 7;
        private const byte colIndexForMessageMetadataPayload = 8;
    }
}