using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Manta.Projections.Construct.TestProjections
{
    [DataContract(Name = "TestProjection")]
    public class TestProjection : Projection,
        IProject<TestContracts.MessageOne>,
        IProject<TestContracts.MessageTwo>
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