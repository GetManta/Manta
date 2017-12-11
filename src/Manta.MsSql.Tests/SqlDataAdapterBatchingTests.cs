using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Manta.MsSql.Tests.Infrastructure;
using Xunit;

namespace Manta.MsSql.Tests
{
    public class SqlDataAdapterBatchingTests : TestsBase
    {
        public SqlDataAdapterBatchingTests(LocalDbFixture fixture) : base(fixture) { }

        [Fact]
        public async Task Batching_availability_check()
        {
            var exception = await Record.ExceptionAsync(async () =>
            {
                var sqlCmdSetType = typeof(SqlDataAdapter);
                var p1 = Expression.Parameter(typeof(object));
                var converted = Expression.Convert(p1, sqlCmdSetType);
                var cmd = Expression.Parameter(typeof(IDbCommand));

                var addToBatchMethod = Expression.Lambda<Func<object, IDbCommand, int>>(Expression.Call(converted, "AddToBatch", null, cmd), p1, cmd).Compile();
                var initializeBatchingMethod = Expression.Lambda<Action<object>>(Expression.Call(converted, "InitializeBatching", null, null), p1).Compile();
                var terminateBatchingMethod = Expression.Lambda<Action<object>>(Expression.Call(converted, "TerminateBatching", null, null), p1).Compile();
                var executeBatchMethod = Expression.Lambda<Func<object, int>>(Expression.Call(converted, "ExecuteBatch", null, null), p1).Compile();

                await GetMessageStore();

                using (var cnn = new SqlConnection(ConnectionString))
                {
                    var instance = new SqlDataAdapter(cnn.CreateCommand());

                    initializeBatchingMethod(instance);

                    // First command for batch
                    var command = new SqlCommand("UPDATE Streams SET [MessagePosition] = 0 WHERE [MessagePosition] = 0", cnn);
                    addToBatchMethod(instance, command);

                    // Second command for batch
                    command = new SqlCommand("UPDATE Streams SET [MessagePosition] = 1 WHERE [MessagePosition] = 1", cnn);
                    addToBatchMethod(instance, command);

                    cnn.Open();

                    executeBatchMethod(instance);

                    terminateBatchingMethod(instance);
                }
            });

            Assert.Null(exception); // Batching doesn't work on .Net Core (for now)
        }
    }
}
