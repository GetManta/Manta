namespace Manta.Projections.MsSql.SqlScripts
{
    internal static class Initialize
    {
        static Initialize()
        {
            // Load embeded query
            Query = Resources.Read("Manta.Projections.MsSql.SqlScripts.Initialize.sql");
        }

        public static string Query { get; }
    }
}
