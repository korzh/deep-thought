using System.Collections.Generic;


namespace Korzh.NLP.DocIndex {
    public class DocIndexBuilder
    {
        private DocBaseIndex _docIndex;

        public DocIndexBuilder() {
            _docIndex = new DocBaseIndex();
        }

        public DocIndexBuilder(DocBaseIndex docIndex) {
            _docIndex = docIndex;
        }

        public DocBaseIndex DocIndex => _docIndex;

        public void RefreshDocBaseIndex(IEnumerable<Document> documents, string lang) {
            var keywordExtractor = new KeywordExtractor(lang);

            foreach (var document in documents) {
                AddDocument(document, keywordExtractor);
            }

            _docIndex.Recalculate();
        }

        public void AddDocument(Document document, KeywordExtractor keywordExtractor) {
            _docIndex.AddDocument(document.Id);


            //add document's title
            var atokenizer = new TextTokenizer(document.Title, TextFormats.WordsOnly, keywordExtractor);
            string atoken = atokenizer.FirstToken();
            while (atoken != null) {
                _docIndex.AddWordToDoc(document.Id, atoken, 10);
                atoken = atokenizer.NextToken();
            }

            //add document's content
            if (document.Text != null) {
                atokenizer = new TextTokenizer(document.Text, TextFormats.WordsOnly, keywordExtractor);
                atoken = atokenizer.FirstToken();
                while (atoken != null) {
                    _docIndex.AddWordToDoc(document.Id, atoken);
                    atoken = atokenizer.NextToken();
                }
            }
        }

        public void AddQuestion(Question question, KeywordExtractor keywordExtractor) {
            if (question.DocId == null) {
                //we can't add questions which are not answered yet.
                return;
            }

            //add question's title
            var qtokenizer = new TextTokenizer(question.Title, TextFormats.WordsOnly, keywordExtractor);
            string qtoken = qtokenizer.FirstToken();
            while (qtoken != null) {
                _docIndex.AddWordToDoc(question.DocId, qtoken, 10);
                qtoken = qtokenizer.NextToken();
            }

            //add question's content
            if (question.Content != null) {
                qtokenizer = new TextTokenizer(question.Content, TextFormats.WordsOnly, keywordExtractor);
                qtoken = qtokenizer.FirstToken();
                while (qtoken != null) {
                    _docIndex.AddWordToDoc(question.DocId, qtoken);
                    qtoken = qtokenizer.NextToken();
                }
            }
        }
    }
}
