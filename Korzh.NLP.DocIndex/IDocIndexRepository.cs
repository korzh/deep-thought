using System.Threading.Tasks;

namespace Korzh.NLP.DocIndex 
{
    public interface IDocBaseIndexRepository
    {
        Task SaveAsync(string docIndexId, DocBaseIndex docIndex);

        Task<DocBaseIndex> LoadAsync(string docIndexId);

        Task RemoveAsync(string docIndexId);
    }
}
