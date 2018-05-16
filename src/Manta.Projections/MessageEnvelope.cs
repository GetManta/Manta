namespace Manta.Projections
{
    public class MessageEnvelope
    {
        public Metadata Meta { get; set; }
        public object Message { get; set; }
    }
}