using System;
using System.Data.SqlLocalDb;

namespace Manta.MsSql.Tests.Infrastructure
{
    public class LocalDbFixture : IDisposable
    {
        public LocalDbFixture()
        {
            // Do "global" initialization here; Only called once.
            LocalDb = LocalDbInstanceProvider.Current.Start(typeof(TestsBase).Assembly.FullName);
        }

        public void Dispose()
        {
            // Do "global" teardown here; Only called once.
            LocalDbInstanceProvider.Current.Stop();
        }

        public ISqlLocalDbInstance LocalDb { get; private set; }
    }
}