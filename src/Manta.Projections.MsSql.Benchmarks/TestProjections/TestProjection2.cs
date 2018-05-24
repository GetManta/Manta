using System.Runtime.Serialization;
using System.Threading.Tasks;
using Benchmarks.Shared;

namespace Manta.Projections.MsSql.Benchmarks.TestProjections
{
    [DataContract(Name = "TestProjection2")]
    public class TestProjection2 : Projection,
        IProjecting<TestContracts.MessageOne>,
        IProjecting<TestContracts.MessageTwo>
    {
        public Task On(TestContracts.MessageOne m, IMetadata meta, ProjectingContext context)
        {
            return Task.CompletedTask;
        }

        public Task On(TestContracts.MessageTwo m, IMetadata meta, ProjectingContext context)
        {
            return Task.CompletedTask;
        }
    }
}