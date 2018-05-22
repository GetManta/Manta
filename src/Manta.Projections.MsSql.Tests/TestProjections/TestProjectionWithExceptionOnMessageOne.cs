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
        public Task On(TestContracts.MessageOne m, Metadata meta, ProjectingContext context)
        {
            throw new Exception("Should drop!");
        }

        public Task On(TestContracts.MessageTwo m, Metadata meta, ProjectingContext context)
        {
            return Task.CompletedTask;
        }
    }
}