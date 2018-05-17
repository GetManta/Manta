using System;
using System.Collections.Generic;

namespace Manta.Projections
{
    public class Runner : IDisposable
    {
        private readonly List<RunnerContext> _projectors;

        public Runner()
        {
            _projectors = new List<RunnerContext>();
        }

        public void Add(ProjectorBase projector, TimeSpan? runForDuration = null)
        {
            _projectors.Add(new RunnerContext(projector, runForDuration));
        }

        public void Start()
        {
            foreach (var projector in _projectors)
            {
                projector.Start();
            }
        }

        public void Stop()
        {
            foreach (var projector in _projectors)
            {
                projector.Stop();
            }
        }

        public void Dispose()
        {
            foreach (var projector in _projectors)
            {
                projector.Dispose();
            }
        }
    }
}