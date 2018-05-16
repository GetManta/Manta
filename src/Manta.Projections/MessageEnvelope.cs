namespace Manta.Projections
{
    public class MessageEnvelope
    {
        public MessageEnvelope(Metadata meta, object message)
        {
            Meta = meta;
            Message = message;
        }

        public Metadata Meta { get; }
        public object Message { get; }
    }
}