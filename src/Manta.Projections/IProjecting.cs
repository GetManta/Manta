using System.Threading.Tasks;

namespace Manta.Projections
{
    public interface IProjecting<in TMessage>
    {
        Task On(TMessage m, Metadata meta, ProjectingContext context);
    }
}