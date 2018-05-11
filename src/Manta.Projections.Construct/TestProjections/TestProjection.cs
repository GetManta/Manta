using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Manta.Projections.Construct.TestProjections
{
    [DataContract(Name = "TestProjection")]
    public class TestProjection : Projection,
        IProject<TestEvent1>,
        IProject<TestEvent2>,
        IProject<object>
    {
        public Task On(TestEvent1 m, Metadata meta, ProjectingContext context)
        {
            Console.WriteLine("On: " + m.GetType().Name);
            return Task.CompletedTask;
        }

        public Task On(TestEvent2 m, Metadata meta, ProjectingContext context)
        {
            if (context.RetryAttempt > 3) context.Drop();

            Console.WriteLine("On: " + m.GetType().Name);
            return Task.CompletedTask;
        }

        public Task On(object m, Metadata meta, ProjectingContext context)
        {
            Console.WriteLine("On " + meta.MessagePosition + ": " + m.GetType().Name);
            return Task.CompletedTask;
        }
    }
}