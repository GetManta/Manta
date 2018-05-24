using System;
using System.Collections.Generic;
using System.Linq;
using Manta.Sceleton.Installer;

namespace Manta.Projections.MsSql.SqlScripts
{
    internal static class Scripts
    {
        static Scripts()
        {
            Queries = new Dictionary<string, string>
            {
                { "1.0.0", Resources<MsSqlProjector>.Read("Manta.Projections.MsSql.SqlScripts.Script.1.0.0.sql") }
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
