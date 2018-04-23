using System;
using System.Collections.Generic;
using System.Linq;

namespace Korzh.NLP.DocIndex
{
    public class SmartTextClassifier {
        private KeywordExtractor _keywordExtractor;
        private DocBaseIndex _docIndex;

        public SmartTextClassifier(string lang, DocBaseIndex docIndex) {
            _keywordExtractor = new KeywordExtractor(lang);
            _docIndex = docIndex;
        }

        public IEnumerable<DocPrediction> SuggestDocs(string text, int count) {
            //find relevant articles
            var tokenizer = new TextTokenizer(text, TextFormats.WordsOnly, _keywordExtractor);
            var docsDict = new Dictionary<DocData, double>();

            //here we calculate the "score" of each document in our DB as the SUM of TFIDF of all words found in the text
            string token = tokenizer.FirstToken();
            while (token != null) {
                if (_docIndex.Words.TryGetValue(token, out WordData wd)) {
                    foreach (var wdse in wd.DocScores) {
                        var dd = _docIndex.Documents[wdse.ClassIndex];
                        if (!docsDict.TryGetValue(dd, out double score)) {
                            score = 0;
                        }
                        score += wdse.TFIDF;
                        docsDict[dd] = score;
                    }
                }
                token = tokenizer.NextToken();
            }

            var result = new List<DocPrediction>();

            foreach (var dde in docsDict) {
                var dd = dde.Key;
                result.Add(new DocPrediction(dd.DocumentId, dde.Value));
            }

            return result
                    .OrderByDescending(dp => dp.Relevance)
                    .ToList()
                    .Take(count);
        }
    }


    public class DocPrediction {
        public string DocId;

        public double Relevance;

        public DocPrediction(string id, double relevance) {
            DocId = id;
            Relevance = relevance;
        }
    }
}
