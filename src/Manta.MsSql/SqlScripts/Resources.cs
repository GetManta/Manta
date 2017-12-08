using System.IO;

namespace Manta.MsSql.SqlScripts
{
    public static class Resources
    {
        public static string Read(string resourceName)
        {
            var assem = typeof(MsSqlMessageStore).Assembly;
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