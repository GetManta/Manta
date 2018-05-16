using System.IO;

namespace Manta.Projections.MsSql.SqlScripts
{
    public static class Resources
    {
        public static string Read(string resourceName)
        {
            var assem = typeof(MsSqlDataSource).Assembly;
            using (var stream = assem.GetManifestResourceStream(resourceName))
            {
                if (stream == null) return null;
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}