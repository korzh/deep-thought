#pragma warning disable SG0018 // Path traversal
#pragma warning disable SEC0112 // Path Tampering Unvalidated File Path

using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Korzh.NLP.DocIndex {
    public class DocBaseIndexFileRepository : IDocBaseIndexRepository {

        private DocBaseIndexBinaryStorage _binaryStorage;

        public DocBaseIndexFileRepository(string folderPath) {
            _binaryStorage = new DocBaseIndexBinaryStorage();
            FolderPath = folderPath;
        }

        public string FolderPath { get; set; } = "";

        private string GetFilePath(string docIndexId) {
            return Path.Combine(FolderPath, docIndexId + ".adbi");
        }

        public async Task<DocBaseIndex> LoadAsync(string docIndexId) {
            var filePath = GetFilePath(docIndexId);
            if (File.Exists(filePath)) {
                return await _binaryStorage.LoadFromStreamAsync(new FileStream(filePath, FileMode.Open));
            }
            else {
                return null;
            }
        }

        public async Task SaveAsync(string docIndexId, DocBaseIndex docIndex) {
            if (!string.IsNullOrEmpty(FolderPath)) {
                Directory.CreateDirectory(FolderPath);
            }
            var filePath = GetFilePath(docIndexId);

            await _binaryStorage.SaveToStreamAsync(docIndex, new FileStream(filePath, FileMode.Create));
        }

        public Task RemoveAsync(string docIndexId) {
            var filePath = GetFilePath(docIndexId);
            if (File.Exists(filePath)) {
                File.Delete(filePath);
            }
            return Task.CompletedTask;
        }
    }
}
#pragma warning restore SEC0112 // Path Tampering Unvalidated File Path
#pragma warning restore SG0018 // Path traversal

