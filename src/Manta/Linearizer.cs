﻿using System;
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
            _timer = new System.Timers.Timer(timeout.TotalMilliseconds) { AutoReset = false, SynchronizingObject = null, Site = null };
            _timer.Elapsed += (s, e) => Execute().SwallowException();
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
            _startedAt.Set(DateTime.UtcNow);
            if (_isWorking) return;
            _isWorking = true;
            _timer.Start();
            Logger.Debug("Linearizer for duration {0} minute(s) started.", Math.Round(WorkDuration.TotalMinutes, 2));
        }

        /// <inheritdoc />
        public async Task RunNow()
        {
            try
            {
                if (_isWorking) return;
                _isWorking = true;
                while (true)
                {
                    if (!(await Linearize(_disposedTokenSource.Token).NotOnCapturedContext())) break;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
            finally
            {
                _isWorking = false;
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

        private async Task Execute()
        {
            if (ShouldStop())
            {
                Stop();
            }
            else
            {
                try
                {
                    while (true)
                    {
                        if (!(await Linearize(_disposedTokenSource.Token).NotOnCapturedContext())) break;
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e.ToString());
                }
                finally
                {
                    _timer.Start();
                }
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