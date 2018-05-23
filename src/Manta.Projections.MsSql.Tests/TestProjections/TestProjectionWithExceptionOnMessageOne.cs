using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Manta.Projections.MsSql.Tests.TestProjections
{
    [DataContract(Name = "TestProjectionWithExceptionOnMessageOne")]
    public class TestProjectionWithExceptionOnMessageOne : Projection,
        IProjecting<TestContracts.MessageOne>,
        IProjecting<TestContracts.MessageTwo>
    {
        public Task On(TestContracts.MessageOne m, IMetadata meta, ProjectingContext context)
        {
            throw new Exception("Should drop!");
        }

        public Task On(TestContracts.MessageTwo m, IMetadata meta, ProjectingContext context)
        {
            return Task.CompletedTask;
        }
    }
}