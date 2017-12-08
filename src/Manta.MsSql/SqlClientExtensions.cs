using System;
using System.Data.SqlClient;

namespace Manta.MsSql
{
    internal static class SqlClientExtensions
    {
        private const short duplicateKeyViolationErrorNumber = 2627;
        private const short duplicateUniqueIndexViolationErrorNumber = 2601;
        private const string wrongExpectedVersionKey = "WrongExpectedVersion";

        public static bool IsUniqueConstraintViolation(this SqlException e, string indexName = null)
        {
            return (e.Number == duplicateKeyViolationErrorNumber || e.Number == duplicateUniqueIndexViolationErrorNumber)
                && (indexName == null || e.Message.Contains($"'{indexName}'"));
        }

        public static bool IsWrongExpectedVersionRised(this SqlException e)
        {
            return e.Message.StartsWith(wrongExpectedVersionKey, StringComparison.OrdinalIgnoreCase);
        }
    }
}
