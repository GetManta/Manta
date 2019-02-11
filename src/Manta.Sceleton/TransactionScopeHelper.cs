using System.Transactions;

namespace Manta.Sceleton
{
    public static class TransactionScopeHelper
    {
        private static readonly TransactionOptions transactionOptions = new TransactionOptions
        {
            IsolationLevel = IsolationLevel.ReadCommitted
        };

        public static TransactionScope New(TransactionScopeOption scopeOption = TransactionScopeOption.RequiresNew)
        {
            return new TransactionScope(
                scopeOption,
                transactionOptions,
                TransactionScopeAsyncFlowOption.Enabled);
        }
    }
}