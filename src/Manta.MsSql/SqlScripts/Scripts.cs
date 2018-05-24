using System;
using System.Collections.Generic;
using System.Linq;
using Manta.Sceleton.Installer;

namespace Manta.MsSql.SqlScripts
{
    internal static class Scripts
    {
        static Scripts()
        {
            Queries = new Dictionary<string, string>
            {
                { "1.0.0", Resources<MsSqlMessageStore>.Read("Manta.MsSql.SqlScripts.Script.1.0.0.sql") },
                { "1.0.1", Resources<MsSqlMessageStore>.Read("Manta.MsSql.SqlScripts.Script.1.0.1.sql") }
            };
        }

        private static Dictionary<string, string> Queries { get; }

        public static string[] GetScriptsFrom(Version version = null)
        {
            return version == null
                ? Queries.Select(x => x.Value).ToArray()
                : Queries.Where(x => Version.Parse(x.Key) > version).Select(x => x.Value).ToArray();
        }
    }
}
