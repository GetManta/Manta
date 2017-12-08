using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Manta.MsSql
{
    internal class SqlClientSqlCommandSet : IDisposable
    {
        private static readonly Type sqlCmdSetType;
        private readonly object _instance;

        private static readonly Action<object, SqlConnection> setConnection;
        private static readonly Func<object, SqlConnection> getConnection;
        private static readonly Action<object, SqlTransaction> setTransaction;
        private static readonly Func<object, SqlCommand> getCommand;
        private static readonly Action<object, SqlCommand> appendMethod;
        private static readonly Func<object, int> executeNonQueryMethod;
        private static readonly Action<object> disposeMethod;

        static SqlClientSqlCommandSet()
        {
            var sysData = typeof(SqlCommand).Assembly;
            sqlCmdSetType = sysData.GetType("System.Data.SqlClient.SqlCommandSet");
            Debug.Assert(sqlCmdSetType != null, "Could not find SqlCommandSet!");

            var p1 = Expression.Parameter(typeof(object));
            var converted = Expression.Convert(p1, sqlCmdSetType);

            var con = Expression.Parameter(typeof(SqlConnection));
            var tran = Expression.Parameter(typeof(SqlTransaction));
            var cmd = Expression.Parameter(typeof(SqlCommand));

            setConnection = Expression.Lambda<Action<object, SqlConnection>>(Expression.Call(converted, "set_Connection", null, con), p1, con).Compile();
            getConnection = Expression.Lambda<Func<object, SqlConnection>>(Expression.Call(converted, "get_Connection", null), p1).Compile();
            setTransaction = Expression.Lambda<Action<object, SqlTransaction>>(Expression.Call(converted, "set_Transaction", null, tran), p1, tran).Compile();
            getCommand = Expression.Lambda<Func<object, SqlCommand>>(Expression.Call(converted, "get_BatchCommand", null), p1).Compile();
            appendMethod = Expression.Lambda<Action<object, SqlCommand>>(Expression.Call(converted, "Append", null, cmd), p1, cmd).Compile();
            executeNonQueryMethod = Expression.Lambda<Func<object, int>>(Expression.Call(converted, "ExecuteNonQuery", null), p1).Compile();
            disposeMethod = Expression.Lambda<Action<object>>(Expression.Call(converted, "Dispose", null), p1).Compile();
        }

        public SqlClientSqlCommandSet(SqlConnection connection = null)
        {
            _instance = Activator.CreateInstance(sqlCmdSetType, true);
            Connection = connection;
        }

        /// <summary>
        /// Append a command to the batch
        /// </summary>
        /// <param name="command"></param>
        public void Append(SqlCommand command)
        {
            AssertHasParameters(command);
            appendMethod(_instance, command);
            CountOfCommands++;
        }

        /// <summary>
        /// This is required because SqlClient.SqlCommandSet will throw if 
        /// the command has no parameters.
        /// </summary>
        /// <param name="command"></param>
        private static void AssertHasParameters(SqlCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            if (command.Parameters.Count == 0)
            {
                throw new ArgumentException("A command in SqlCommandSet must have parameters. You can't pass hardcoded sql strings.");
            }
        }

        /// <summary>
        /// Return the batch command to be executed
        /// </summary>
        public SqlCommand BatchCommand
        {
            get { return getCommand(_instance); }
        }

        /// <summary>
        /// The number of commands batched in this instance
        /// </summary>
        public int CountOfCommands { get; private set; }

        /// <summary>
        /// Executes the batch
        /// </summary>
        /// <returns>
        /// This seems to be returning the total number of affected rows in all queries
        /// </returns>
        public int ExecuteNonQuery()
        {
            if (Connection == null)
                throw new ArgumentNullException(nameof(Connection),
                    "Connection was not set! You must set the connection property before calling ExecuteNonQuery()");

            return CountOfCommands == 0 ? 0 : executeNonQueryMethod(_instance);
        }

        public Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
        {
            if (Connection == null)
                throw new ArgumentNullException(nameof(Connection),
                    "Connection was not set! You must set the connection property before calling ExecuteNonQuery()");

            return CountOfCommands == 0 ? Task.FromResult(0) : Task.Run(() => executeNonQueryMethod(_instance), cancellationToken);
        }

        public SqlConnection Connection
        {
            get { return getConnection(_instance); }
            set { setConnection(_instance, value); }
        }

        public SqlTransaction Transaction
        {
            set { setTransaction(_instance, value); }
        }

        /// <inheritdoc />
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            disposeMethod(_instance);
        }
    }
}
