#pragma warning disable IDE0012 // Name can be simplified

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Korzh.NLP.DocIndex
{
    public class DocBaseIndex {

        readonly IDictionary<string, WordData> _words;

        public IDictionary<string, WordData> Words => _words;

        readonly IList<DocData> _docs;

        public IList<DocData> Documents => _docs;

        readonly IList<TagData> _tags;

        public IList<TagData> Tags => _tags;


        public IDictionary<string, DocData> _docsDict;

        public IDictionary<string, TagData> _tagsDict;

        public DocBaseIndex() {
            _words = new Dictionary<string, WordData>();
            _docs = new List<DocData>();
            _tags = new List<TagData>();

            _docsDict = new Dictionary<string, DocData>();
            _tagsDict = new Dictionary<string, TagData>();
        }

        public DocData AddDocument(string documentId) {
            if (!_docsDict.TryGetValue(documentId, out DocData docData)) {
                docData = new DocData(documentId);
                AddDocData(docData);

                foreach (var wd in _words.Values) {
                    wd.AddEmptyDocument(docData.Index);
                }
            }

            docData.IncImportance(1);
            return docData;
        }

        internal int AddDocData(DocData docData) {
            _docsDict[docData.DocumentId] = docData;
            _docs.Add(docData);
            docData.Index = _docs.Count - 1;
            return docData.Index;
        }

        public void AddArticleTags(string[] tags) {
            double value = 2d;
            foreach (var tag in tags) {
                AddTag(tag, value);
            }
        }

        public TagData AddTag(string tag, double value) {
            if (!_tagsDict.TryGetValue(tag, out TagData tagData)) {
                tagData = new TagData(tag);
                AddTagData(tagData);

                foreach (var wd in _words.Values) {
                    wd.AddEmptyTag(tagData.Index);
                }
            }
            tagData.IncImportance(value);
            return tagData;
        }

        internal int AddTagData(TagData tagData) {
            _tagsDict[tagData.Tag] = tagData;
            _tags.Add(tagData);
            tagData.Index = _tags.Count - 1;
            return tagData.Index;
        }

        public void RemoveTag(string tag) {
            if (_tagsDict.TryGetValue(tag, out TagData tagData)) {
                foreach (var we in _words) {
                    we.Value.TagScores.RemoveAt(tagData.Index);
                }

                _tagsDict.Remove(tag);
                _tags.RemoveAt(tagData.Index);
            }
        }


        public void AddWordToDoc(string docId, string word, double wordValue = 1d) {
            var docData = AddDocument(docId);
            docData.IncTotalWordScore(wordValue);

            if (!_words.TryGetValue(word, out WordData wordData)) {
                wordData = new WordData(_docs, _tags);
                _words[word] = wordData;
            }

            var wordScore = wordData.DocScores[docData.Index];
            wordScore.IncScore(wordValue);
        }

        internal void AddWordWithData(string word, WordData wordData) {
            _words[word] = wordData;
        }

        public void AddWordToTags(string[] tags, string word, double wordValue = 1d) {
            foreach (string tag in tags) {
                var tagData = AddTag(tag, 1);
                tagData.IncTotalWordScore(wordValue);

                if (!_words.TryGetValue(word, out WordData wordData)) {
                    wordData = new WordData(_docs, _tags);
                    _words[word] = wordData;
                }

                var wordScore = wordData.TagScores[tagData.Index];
                wordScore.IncScore(wordValue);
            }
        }

        public void Recalculate() {
            foreach (var wd in _words.Values) {
                wd.Recalculate(_docs, _tags);
            }
        }
    }


    /// <summary>
    /// Contains necessary data for one document
    /// </summary>
    public class DocData {
        public Int32 Index { get; internal set; }

        public string DocumentId { get; internal set; }

        private double _totalWordScore;
        //Total score of all words included in this document
        //In the simplest case - it just the word count
        public double TotalWordScore => _totalWordScore;

        private double _importance = 0;

        public double Importance => _importance;


        public DocData(string docId) {
            DocumentId = docId;
            _totalWordScore = 0f;
        }

        public void IncTotalWordScore(double score) {
            _totalWordScore += score;
        }

        public void IncImportance(double value) {
            _importance += value;
        }

        public string Serialize() {
            return $"{Index},{DocumentId},{Importance.ToDotStr()},{TotalWordScore.ToDotStr()}"; 
        }

        public void Deserialize(string s) {
            ///TODO: implement
        }

        public Task ReadFromBinaryAsync(BinaryReader reader) {
            Index = reader.ReadInt32();
            DocumentId = reader.ReadString();
            _importance = reader.ReadDouble();
            _totalWordScore = reader.ReadDouble();

            return Task.CompletedTask;
        }

        public Task WriteToBinaryAsync(BinaryWriter writer) {
            writer.Write(Index);
            writer.Write(DocumentId);
            writer.Write(Importance);
            writer.Write(TotalWordScore);

            return Task.CompletedTask;
        }
    }

    public class TagData {

        public string Tag {get; internal set;}

        public int Index { get; internal set; }

        private double _importance = 0;

        public double Importance => _importance;

        private double _totalWordScore = 0;
        public double TotalWordScore => _totalWordScore;

        public TagData(string tag) {
            Tag = tag;
        }

        public void IncTotalWordScore(double score) {
            _totalWordScore += score;
        }

        public void IncImportance(double value) {
            _importance += value;
        }

        public string Serialize() {
            return $"{Index},{Tag},{Importance.ToDotStr()},{TotalWordScore.ToDotStr()}";
        }

        public void Deserialize(string s) {
            ///TODO: implement
        }

        internal Task ReadFromBinaryAsync(BinaryReader reader) {
            Index = reader.ReadInt32();
            Tag = reader.ReadString();
            _importance = reader.ReadDouble();
            _totalWordScore = reader.ReadDouble();

            return Task.CompletedTask;
        }

        public Task WriteToBinaryAsync(BinaryWriter writer) {
            writer.Write(Index);
            writer.Write(Tag);
            writer.Write(Importance);
            writer.Write(TotalWordScore);

            return Task.CompletedTask;
        }
    }


    public class WordData {
        readonly IList<WordClassScores> _docScores;
        readonly IList<WordClassScores> _tagScores;

        public IList<WordClassScores> TagScores => _tagScores;

        public IList<WordClassScores> DocScores => _docScores;

        public WordData(IList<DocData> docs, IList<TagData> tags) {
            _docScores = new List<WordClassScores>();
            if (docs != null) {
                foreach (var dd in docs) {
                    _docScores.Add(new WordClassScores(dd.Index));
                }
            }

            _tagScores = new List<WordClassScores>();
            if (tags != null) {
                foreach (var td in tags) {
                    _tagScores.Add(new WordClassScores(td.Index));
                }
            }
        }

        public void AddEmptyDocument(Int32 docIndex) {
            _docScores.Add(new WordClassScores(docIndex));
        }

        public void AddEmptyTag(int tagIndex) {
            _tagScores.Add(new WordClassScores(tagIndex));
        }

        public void Recalculate(IList<DocData> allDocs, IList<TagData> allTags) {
            double totalDocsWithWord = _docScores.Count(wds => wds.Score > 0);

            double docsIDF = (totalDocsWithWord > 0) ?
                    Math.Log10((double)(allDocs.Count) / totalDocsWithWord) :
                    0;

            int i = 0;
            foreach (var wds in _docScores) {
                var dd = allDocs[wds.ClassIndex];
                wds.Recalculate(dd.TotalWordScore, docsIDF);
                i++;
            }

            double totalTagsWithWord = _tagScores.Count(wts => wts.Score > 0);

            double tagsIDF = (totalTagsWithWord > 0) ?
                    Math.Log10((double)(allTags.Count) / totalTagsWithWord) :
                    0;

            foreach (var wts in _tagScores) {
                var td = allTags[wts.ClassIndex];
                wts.Recalculate(td.TotalWordScore, tagsIDF);
                i++;
            }
        }

        public string Serialize() {
            return string.Join(";", DocScores.Select(ds => ds.Serialize())) + "\n"
                   + string.Join(";", TagScores.Select(ts => ts.Serialize()));
        }

        public void Deserialize(string s) {
            ///TODO: to implement
        }

        public async Task ReadFromBinaryAsync(BinaryReader reader) {
            Int32 docCount = reader.ReadInt32();
            for (int i = 0; i < docCount; i++) {
                var wcs = new WordClassScores(i);
                await wcs.ReadFromBinaryAsync(reader);
                DocScores.Add(wcs);
            }

            Int32 tagCount = reader.ReadInt32();
            for (int i = 0; i < tagCount; i++) {
                var wcs = new WordClassScores(i);
                await wcs.ReadFromBinaryAsync(reader);
                TagScores.Add(wcs);
            }
        }

        public async Task WriteToBinaryAsync(BinaryWriter writer) {
            //writing documents
            Int32 len = DocScores.Count;
            writer.Write(len);
            foreach (var ds in DocScores) {
                await ds.WriteToBinaryAsync(writer);
            }

            //writing tags
            len = TagScores.Count;
            writer.Write(len);
            foreach (var ts in TagScores) {
                await ts.WriteToBinaryAsync(writer);
            }
        }
    }


    public class WordClassScores {
        //The index of the class (document, tag, etc)
        public int ClassIndex { get; internal set; }

        //The value (score) of this word within this class
        //In the basic case it's just the number of word occurencies
        //However, it's not true in a general case since 
        private double _score;

        //term frequency 
        //Number of times this word occurs in this class divide on total number of words in this class)
        //Isn't it the same as likehood in Naive Bayes???
        private double _tf;

        //TD-IDF
        private double _tfidf;

        public WordClassScores(int classIndex) {
            ClassIndex = classIndex;
            _score = 0;
            _tf = 0d;
            _tfidf = 0d;
        }

        public double Score => _score;


        public void IncScore(double scoreShift) => 
            _score += scoreShift;

        public void Recalculate(double totalWordScoreInClass, double idf) {
            if (totalWordScoreInClass > 0) {
                _tf = Score / totalWordScoreInClass;
            }

            _tfidf = _tf * idf;
        }

        public double TF => _tf;

        public double TFIDF => _tfidf;

        public string Serialize() {
            return $"{Score.ToDotStr()},{TF.ToDotStr()},{TFIDF.ToDotStr()}";
        }

        public void Deserialize(string s) {
            ///TODO: to implement
        }

        public Task ReadFromBinaryAsync(BinaryReader reader) {
            _score = reader.ReadDouble();
            _tf = reader.ReadDouble();
            _tfidf = reader.ReadDouble();

            return Task.CompletedTask;
        }

        public Task WriteToBinaryAsync(BinaryWriter writer) {
            writer.Write(Score);
            writer.Write(TF);
            writer.Write(TFIDF);

            return Task.CompletedTask;
        }
    }

    public static class SomeExtensions {

        private static object _lockObj = new object();

        public static int SafeAdd<T>(this IList<T> list, T item) {
            lock (_lockObj) {
                list.Add(item);
                return list.Count - 1;
            }
        }

        private static CultureInfo _cultureUS = CultureInfo.GetCultureInfo("en-US");
        public static string ToDotStr(this double d) {
            return d.ToString(_cultureUS);
        }
    }
}
#pragma warning restore IDE0012 // Name can be simplified
