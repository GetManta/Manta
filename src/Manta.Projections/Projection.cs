using System.Threading.Tasks;

namespace Manta.Projections
{
    public abstract class Projection
    {
        public virtual Task Create() { return Task.CompletedTask; }
        public virtual Task Destroy() { return Task.CompletedTask; }
    }
}