using System.Data;

namespace Manta.Projections.MsSql
{
    internal static class AdoExtensions
    {
        public static IDbCommand AddInputParam(this IDbCommand cmd, string name, DbType type, object value, int? size = null)
        {
            var param = cmd.CreateParameter();
            param.DbType = type;
            param.ParameterName = name;
            param.Value = value;
            param.Direction = ParameterDirection.Input;
            if (size != null) param.Size = size.Value;
            cmd.Parameters.Add(param);
            return cmd;
        }
    }
}