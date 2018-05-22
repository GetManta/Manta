namespace Manta.MsSql.SqlScripts
{
    internal static class Initialize
    {
        static Initialize()
        {
            // Load embeded query
            InitializeQuery = Resources.Read("Manta.MsSql.SqlScripts.Initialize.sql");
        }

        public static string InitializeQuery { get; }
    }
}
