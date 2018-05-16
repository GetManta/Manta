namespace Manta.Sceleton.Converters
{
    public interface IUpConvertMessage
    {
        // Marker interface
    }

    public interface IUpConvertMessage<in TSource, out TTarget> : IUpConvertMessage
        where TSource : class
        where TTarget : class
    {
        /// <summary>
        /// Converts an message from one type to another.
        /// </summary>
        /// <param name="sourceMessage">The event to be converted.</param>
        /// <returns>The converted event.</returns>
        TTarget Convert(TSource sourceMessage);
    }
}