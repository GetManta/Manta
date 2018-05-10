using System;
using System.Threading;
using System.Threading.Tasks;
using Manta.Sceleton;

namespace Manta
{
    public abstract class Linearizer : ILinearizer, IDisposable
    {
        private CancellationTokenSource _disposedTokenSource = new CancellationTokenSource();
        private System.Timers.Timer _timer;
        private volatile bool _isWorking;
        private readonly InterlockedDateTime _startedAt;

        protected Linearizer(ILogger logger, TimeSpan timeout, TimeSpan workDuration)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            if (timeout != TimeSpan.Zero)
            {
                _timer = new System.Timers.Timer(timeout.TotalMilliseconds) { AutoReset = false, SynchronizingObject = null, Site = null };
                _timer.Elapsed += (s, e) => ExecuteOnIntervalElapsed().SwallowException();
            }
            WorkDuration = workDuration;
            Timeout = timeout;
            _startedAt = new InterlockedDateTime(DateTime.MaxValue);
        }

        /// <inheritdoc />
        public bool IsWorking => _isWorking;

        /// <inheritdoc />
        public TimeSpan WorkDuration { get; }

        /// <inheritdoc />
        public TimeSpan Timeout { get; }

        /// <inheritdoc />
        public void Start()
        {
            if (_timer == null) return;
            if (Timeout == TimeSpan.Zero) throw new InvalidOperationException("Set Timeout greater than Zero.");

            _startedAt.Set(DateTime.UtcNow);
            if (_isWorking) return;
            _isWorking = true;
            _timer.Start();
            Logger.Debug("Linearizer for duration {0} minute(s) started.", Math.Round(WorkDuration.TotalMinutes, 2));
        }

        /// <inheritdoc />
        public async Task Run()
        {
            if (_isWorking) return;
            _isWorking = true;
            await RunUntilDone().NotOnCapturedContext();
            _isWorking = false;
        }

        private async Task RunUntilDone()
        {
            try
            {
                while (true)
                {
                    var shouldDoMoreWork = await Linearize(_disposedTokenSource.Token).NotOnCapturedContext();
                    if (!shouldDoMoreWork) break;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
        }

        /// <inheritdoc />
        public void Stop()
        {
            if (_timer == null) return;
            _timer.Stop();
            _isWorking = false;
            _startedAt.Set(DateTime.MaxValue);
            Logger.Debug("Linearizer stopped.");
        }

        /// <summary>
        /// Execute linearize method
        /// </summary>
        /// <param name="cancellationToken">Cancelation token</param>
        /// <returns>True if there is more work to do otherwise false.</returns>
        protected abstract Task<bool> Linearize(CancellationToken cancellationToken);

        protected ILogger Logger { get; }

        private async Task ExecuteOnIntervalElapsed()
        {
            if (ShouldStop())
            {
                Stop();
            }
            else
            {
                await RunUntilDone().NotOnCapturedContext();

                _timer?.Start();
            }
        }

        private bool ShouldStop() => (DateTime.UtcNow - _startedAt.Value) > WorkDuration;

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_timer == null) return;
            if (disposing)
            {
                if (_timer != null)
                {
                    if (_timer.Enabled)
                    {
                        Stop();
                    }
                    _timer.Dispose();
                }
                _disposedTokenSource.Dispose();
            }
            _disposedTokenSource = null;
            _timer = null;
        }
    }
}