namespace Manta.Projections
{
    public class MessageEnvelope
    {
        public Metadata Meta { get; set; }
        public object Event { get; set; }
    }
}