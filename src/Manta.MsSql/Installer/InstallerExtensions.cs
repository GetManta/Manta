using System;
using System.Data;
using System.Data.SqlClient;

namespace Manta.MsSql.Installer
{
    internal static class InstallerExtensions
    {
        private const string paramVersion = "@Version";

        private const string mantaSetVersion = @"
EXEC sys.sp_updateextendedproperty 
    @name = 'Version', @VALUE = @Version,
    @level0type = 'SCHEMA', @level0name = 'dbo', 
    @level1type = 'Table', @level1name = 'Streams';";

        private const string mantaGetVersion = @"
SELECT 
    p.value
FROM
    sys.extended_properties AS p
    INNER JOIN sys.TABLES tbl ON tbl.object_id = p.major_id
WHERE
    p.name <> 'MS_Description' AND p.class = 1 AND tbl.name = 'Streams';";

        public static SqlCommand CreateCommandForGetVersion(this SqlConnection cnn)
        {
            return cnn.CreateTextCommand(mantaGetVersion);
        }

        public static SqlCommand CreateCommandForSetVersion(this SqlConnection cnn, Version version)
        {
            return cnn.CreateTextCommand(mantaSetVersion)
                .AddInputParam(paramVersion, SqlDbType.VarChar, version.ToString(3), 10);
        }
    }
}
