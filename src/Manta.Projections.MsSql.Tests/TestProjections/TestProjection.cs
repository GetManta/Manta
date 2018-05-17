using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Manta.Projections.MsSql.Tests.TestProjections
{
    [DataContract(Name = "TestProjection")]
    public class TestProjection : Projection,
        IProjecting<TestContracts.MessageOne>,
        IProjecting<TestContracts.MessageTwo>
    {
        public Task On(TestContracts.MessageOne m, Metadata meta, ProjectingContext context)
        {
            //Console.WriteLine("On: " + m.GetType().Name);
            return Task.CompletedTask;
        }

        public Task On(TestContracts.MessageTwo m, Metadata meta, ProjectingContext context)
        {
            //Console.WriteLine("On: " + m.GetType().Name);
            return Task.CompletedTask;
        }
    }
}