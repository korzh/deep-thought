#pragma warning disable IDE0012 // Name can be simplified

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Korzh.NLP.DocIndex {
    public class DocBaseIndexBinaryStorage  {

        public DocBaseIndexBinaryStorage() {

        }

        private static UInt16 _lastVersionNum =1;

        private static string _dbiFilePrefix = "ADBI";
        private static string _docsSectionPrefix = "__DOCS";
        private static string _tagsSectionPrefix = "__TAGS";
        private static string _wordsSectionPrefix ="__WORDS";


        public async Task<DocBaseIndex> LoadFromStreamAsync(Stream stream) {
            DocBaseIndex docIndex = new DocBaseIndex();
            using (var reader = new BinaryReader(stream, Encoding.UTF8)) {
                string prefix = reader.ReadString();
                if (prefix != _dbiFilePrefix) {
                    throw new DocIndexReadError("Wrong ADBI file prefix");
                }

                UInt16 version = reader.ReadUInt16();
                if (version > _lastVersionNum) {
                    throw new DocIndexReadError($"Wrong index version {version}.\nThis code supports only the versions below {_lastVersionNum}");

                }
                await LoadDocumentsAsync(reader, docIndex);
                await LoadTagsAsync(reader, docIndex);
                await LoadWordsAsync(reader, docIndex);
            }
            return docIndex;
        }

        private async Task LoadDocumentsAsync(BinaryReader reader, DocBaseIndex docIndex) {
            string prefix = reader.ReadString();
            if (prefix != _docsSectionPrefix) {
                throw new DocIndexReadError("Wrong prefix of Docs section");
            }

            Int32 docCount = reader.ReadInt32();
            for (int i = 0; i < docCount; i++) {
                var docData = new DocData("");
                await docData.ReadFromBinaryAsync(reader);

                docIndex.AddDocData(docData);
            }
        }

        private async Task LoadTagsAsync(BinaryReader reader, DocBaseIndex docIndex) {
            string prefix = reader.ReadString();
            if (prefix != _tagsSectionPrefix) {
                throw new DocIndexReadError("Wrong prefix of Tags section");
            }

            Int32 tagCount = reader.ReadInt32();
            for (int i = 0; i < tagCount; i++) {
                var tagData = new TagData("");
                await tagData.ReadFromBinaryAsync(reader);

                docIndex.AddTagData(tagData);
            }
        }

        private async Task LoadWordsAsync(BinaryReader reader, DocBaseIndex docIndex) {
            string prefix = reader.ReadString();
            if (prefix != _wordsSectionPrefix) {
                throw new DocIndexReadError("Wrong prefix of Words section");
            }
            Int32 count = reader.ReadInt32();

            for (int i = 0; i < count; i++) {
                string word = reader.ReadString();
                var wd = new WordData(null, null);
                await wd.ReadFromBinaryAsync(reader);

                docIndex.AddWordWithData(word, wd);
            }
        }

        public async Task SaveToStreamAsync(DocBaseIndex docIndex, Stream stream) {

            using (var writer = new BinaryWriter(stream, Encoding.UTF8)) {
                writer.Write(_dbiFilePrefix);
                writer.Write(_lastVersionNum);
                await SaveDocumentsAsync(writer, docIndex);
                await SaveTagsAsync(writer, docIndex);
                await SaveWordsAsync(writer, docIndex);
            }
        }

        private async Task SaveDocumentsAsync(BinaryWriter writer, DocBaseIndex docIndex) {
            writer.Write(_docsSectionPrefix);
            Int32 count = docIndex.Documents.Count;
            writer.Write(count);
            foreach (var doc in docIndex.Documents) {
                await doc.WriteToBinaryAsync(writer);
            }
        }


        private async Task SaveTagsAsync(BinaryWriter writer, DocBaseIndex docIndex) {
            writer.Write(_tagsSectionPrefix);
            Int32 count = docIndex.Tags.Count;
            writer.Write(count);
            foreach (var tag in docIndex.Tags) {
                await tag.WriteToBinaryAsync(writer);
            }
        }

        private async Task SaveWordsAsync(BinaryWriter writer, DocBaseIndex docIndex) {
            writer.Write(_wordsSectionPrefix);
            Int32 len = docIndex.Words.Count;
            writer.Write(len);

            foreach (var wde in docIndex.Words) {
                writer.Write(wde.Key);
                await wde.Value.WriteToBinaryAsync(writer);
            }
        }

    }


    public class DocIndexReadError : Exception {
        public DocIndexReadError(string message) : base(message) {
        }
    }
}

#pragma warning restore IDE0012 // Name can be simplified
#pragma warning restore SEC0112 // Path Tampering Unvalidated File Path
#pragma warning restore SG0018 // Path traversal

