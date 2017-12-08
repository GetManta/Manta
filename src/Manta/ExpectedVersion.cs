using System;

namespace Manta
{
    /// <summary>
    /// Represents expected version number.
    /// </summary>
    public static class ExpectedVersion
    {
        private const string any = "Any";
        private const string noStream = "NoStream";

        /// <summary>
        /// This write should not conflict with anything and should always succeed.
        /// </summary>
        public const sbyte Any = -1;

        /// <summary>
        /// The stream being written to should not yet exist. If it does exist treat that as a concurrency problem.
        /// </summary>
        public const sbyte NoStream = 0;

        /// <summary>
        /// Parse stream version to readable string.
        /// </summary>
        /// <param name="version">Given version number.</param>
        /// <returns>Readable version string.</returns>
        public static string Parse(int version)
        {
            if (version < Any) throw new InvalidOperationException($"Forbidden stream version '{version}'");
            switch (version)
            {
                case Any:
                    return any;
                case NoStream:
                    return noStream;
                default:
                    return version.ToString();
            }
        }
    }
}