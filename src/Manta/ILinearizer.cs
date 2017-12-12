namespace Manta
{
    public interface ILinearizer
    {
        void Start();
        void Stop();
        bool IsRunning { get; }
    }
}