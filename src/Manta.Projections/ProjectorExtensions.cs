using System.Collections.Generic;
using System.Transactions;

namespace Manta.Projections
{
    internal static class ProjectorExtensions
    {
        private static readonly TransactionOptions transactionOptions = new TransactionOptions
        {
            IsolationLevel = IsolationLevel.ReadCommitted
        };

        public static IEnumerable<long> GenerateRanges(this Projector p, long min, long max, long range)
        {
            for (var i = min; i <= max; i += range) yield return i;
        }

        public static TransactionScope NewTransactionScope(this Projector p)
        {
            return new TransactionScope(
                TransactionScopeOption.RequiresNew,
                transactionOptions,
                TransactionScopeAsyncFlowOption.Enabled);
        }
    }
}