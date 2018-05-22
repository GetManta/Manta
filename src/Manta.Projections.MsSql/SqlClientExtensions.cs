using System.Data;
using System.Data.SqlClient;

namespace Manta.Projections.MsSql
{
    internal static class SqlClientExtensions
    {
        public static SqlCommand CreateCommand(this SqlConnection cnn, string commandText, int? commandTimeout = null)
        {
            var cmd = cnn.CreateCommand();
            cmd.CommandText = commandText;
            cmd.CommandType = CommandType.StoredProcedure;
            if (commandTimeout != null) cmd.CommandTimeout = commandTimeout.Value;
            return cmd;
        }

        public static SqlCommand CreateTextCommand(this SqlConnection cnn, string commandText, int? commandTimeout = null)
        {
            var cmd = cnn.CreateCommand();
            cmd.CommandText = commandText;
            cmd.CommandType = CommandType.Text;
            if (commandTimeout != null) cmd.CommandTimeout = commandTimeout.Value;
            return cmd;
        }

        public static SqlCommand AddInputParam(this SqlCommand cmd, string name, SqlDbType type, object value, int? size = null)
        {
            var param = cmd.Parameters.Add(name, type);
            param.Value = value;
            param.Direction = ParameterDirection.Input;
            if (size != null) param.Size = size.Value;
            return cmd;
        }
    }
}