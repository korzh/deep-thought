using System;
using System.Collections.Generic;
using System.Linq;

using Korzh.NLP;

namespace Korzh.NLP.DocIndex
{
    public class SmartTextClassifier {
        private KeywordExtractor _keywordExtractor;
        private DocBaseIndex _docIndex;

        public SmartTextClassifier(string lang, DocBaseIndex docIndex) {
            _keywordExtractor = new KeywordExtractor(lang);
            _docIndex = docIndex;
        }

        public void AddTextToModel(string docId, string text, string[] tags, double value) {

        }


        public IEnumerable<DocChunk> SuggestTags(string text, int count) {
            //find relevant tags
            var tokenizer = new TextTokenizer(text, TextFormats.WordsOnly, _keywordExtractor);
            var tagsDict = new Dictionary<string, double>();

            string token = tokenizer.FirstToken();
            while (token != null) {
                if (_docIndex.Words.TryGetValue(token, out WordData wd)) {

                    foreach (var wte in wd.TagScores) {
                        var td = _docIndex.Tags[wte.ClassIndex];
                        if (tagsDict.TryGetValue(td.Tag, out double score)) {
                            score += wte.TFIDF * Math.Log10(td.Importance);
                        }
                        else {
                            score = wte.TFIDF;
                        }
                        tagsDict[td.Tag] = score;
                    }
                }
                token = tokenizer.NextToken();
            }

            return tagsDict
                    .OrderByDescending(e => e.Value)
                    .Take(count)
                    .Select(de => new DocChunk(de.Key, de.Value));
        }

        public IEnumerable<DocChunk> SuggestDocs(string text, int count) {
            //find relevant articles
            var tokenizer = new TextTokenizer(text, TextFormats.WordsOnly, _keywordExtractor);
            var docsDict = new Dictionary<string, double>();

            string token = tokenizer.FirstToken();
            while (token != null) {
                if (_docIndex.Words.TryGetValue(token, out WordData wd)) {
                    foreach (var wdse in wd.DocScores) {
                        var dd = _docIndex.Documents[wdse.ClassIndex];
                        if (docsDict.TryGetValue(dd.DocumentId, out double score)) {
                            score += wdse.TFIDF; // * Math.Log10(dd.Importance);
                        }
                        else {
                            score = wdse.TFIDF;
                        }
                        docsDict[dd.DocumentId] = score;
                    }
                }
                token = tokenizer.NextToken();
            }

            return docsDict
                    .OrderByDescending(e => e.Value)
                    .Take(count)
                    .Select(de => new DocChunk(de.Key, de.Value));
        }
    }


    public class DocChunk {
        public string Id;

        public double Relevance;

        public DocChunk(string id, double relevance) {
            Id = id;
            Relevance = relevance;
        }
    }
}
