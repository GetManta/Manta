using System.Runtime.InteropServices;

namespace Manta
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RecordedStream
    {
        public RecordedStream(RecordedMessage[] messages)
        {
            Messages = messages;
        }

        public RecordedMessage[] Messages { get; }

        public bool IsStreamNotFound()
        {
            return Messages == null || Messages.Length == 0;
        }

        public static RecordedStream Empty()
        {
            return new RecordedStream(new RecordedMessage[0]);
        }
    }
}