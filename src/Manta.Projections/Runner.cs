using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Manta.Sceleton;

namespace Manta.Projections
{
    internal class Runner : IDisposable
    {
        private System.Timers.Timer _runnerTimer;
        private CancellationTokenSource _disposedTokenSource = new CancellationTokenSource();
        private volatile bool _isRunning;
        private readonly InterlockedDateTime _startedAt;
        private readonly TimeoutCalculator _timeoutCalc;

        public ProjectorBase Projector { get; }
        public TimeSpan RunForDuration { get; }

        public Runner(ProjectorBase projector, TimeSpan? runForDuration = null)
        {
            Projector = projector;
            RunForDuration = runForDuration ?? TimeSpan.FromMinutes(1);
            _startedAt = new InterlockedDateTime(DateTime.MaxValue);
            _timeoutCalc = new TimeoutCalculator(TimeSpan.Zero);
            _runnerTimer = CreateTimer(_timeoutCalc);
        }

        private System.Timers.Timer CreateTimer(TimeoutCalculator timeoutCalc)
        {
            var timeout = timeoutCalc.CalculateNext();
            var result = new System.Timers.Timer(timeout) { AutoReset = false };
            result.Elapsed += OnTimerElapsed;
            return result;
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            Run().SwallowException(Projector.Logger);
        }

        public void Start()
        {
            _startedAt.Set(DateTime.UtcNow);
            if (_isRunning) return;
            _runnerTimer.Start();
            _isRunning = true;

            Projector.Logger.Debug("Projector '{0}' for duration {1} minute(s) started.", Projector.Name, RunForDuration.TotalMinutes);
        }

        private async Task Run()
        {
            if (ShouldStop())
            {
                Stop();
            }
            else
            {
                try
                {
                    var result = await Projector.Run(_disposedTokenSource.Token);
                    var anyDispatched = result.Any(x => x.AnyDispatched);
                    if (anyDispatched)
                    {
                        // extend period of work duration
                        _startedAt.Set(DateTime.UtcNow);
                    }

                    _runnerTimer.Interval = _timeoutCalc.CalculateNext(anyDispatched);
                }
                catch (Exception e)
                {
                    Projector.Logger.Fatal("Running projector '{0}' error.\r\n\t{1}", Projector.Name, e.Message);
                }
                finally
                {
                    _runnerTimer.Start();
                }
            }
        }

        private bool ShouldStop()
        {
            return (DateTime.UtcNow - _startedAt.Value) > RunForDuration;
        }

        public void Stop()
        {
            if (_runnerTimer == null) return;
            _runnerTimer.Stop();
            _isRunning = false;
            _startedAt.Set(DateTime.MaxValue);

            Projector.Logger.Debug("Projector '{0}' stopped.", Projector.Name);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_runnerTimer == null) return;
            if (disposing)
            {
                if (_runnerTimer != null)
                {
                    if (_runnerTimer.Enabled)
                    {
                        Stop();
                    }
                    _runnerTimer.Dispose();
                }
                _disposedTokenSource.Dispose();
            }
            _disposedTokenSource = null;
            _runnerTimer = null;
        }
    }
}