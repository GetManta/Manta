using System;

namespace Manta.MsSql
{
    internal static class Guard
    {
        /// <summary>
        /// Throws if conditions are met.
        /// </summary>
        /// <param name="value">Stream name</param>
        /// <param name="parameterName">Method parameter name</param>
        public static void StreamName(string value, string parameterName)
        {
            if (value == null) throw new ArgumentNullException(parameterName, "Stream name must be set.");

            if (value.Length > SqlClientExtensions.DefaultStreamNameLength)
                throw new ArgumentException($"Stream name can not be longer than {SqlClientExtensions.DefaultStreamNameLength}.", parameterName);
        }

        /// <summary>
        /// Throws if conditions are met.
        /// </summary>
        /// <param name="value">Contract name</param>
        /// <param name="parameterName">Method parameter name</param>
        public static void ContractName(string value, string parameterName)
        {
            if (value == null) throw new ArgumentNullException(parameterName, "Contract name must be set.");

            if (value.Length > SqlClientExtensions.DefaultContractNameLength)
                throw new ArgumentException($"Contract name can not be longer than {SqlClientExtensions.DefaultContractNameLength}.", parameterName);
        }
    }
}