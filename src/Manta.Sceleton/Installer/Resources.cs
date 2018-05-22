using System.IO;

namespace Manta.Sceleton.Installer
{
    public static class Resources<TSourceType>
    {
        public static string Read(string resourceName)
        {
            var source = typeof(TSourceType).Assembly;
            using (var stream = source.GetManifestResourceStream(resourceName))
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