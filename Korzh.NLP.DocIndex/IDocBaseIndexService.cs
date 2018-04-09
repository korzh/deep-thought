using System.Threading.Tasks;

namespace Korzh.NLP.DocIndex
{
    public interface IDocBaseIndexService {
        Task<DocBaseIndex> LoadDocBaseIndexAsync(string subscriberId, string kbId);

        Task InvalidateDocBaseIndexAsync(string subscriberId, string kbId);
    }
}
