using System.Collections.Generic;


namespace Korzh.NLP.DocIndex
{
    public class DocIndexBuilder
    {

        public DocIndexBuilder() {
        }


        public DocBaseIndex RefreshDocBaseIndex(IEnumerable<Document> documents, string lang) {
            var keywordExtractor = new KeywordExtractor(lang);

            var docIndex = new DocBaseIndex();

            foreach (var document in documents) {
                docIndex.AddDocument(document.Id);

                var atokenizer = new TextTokenizer(document.Title, TextFormats.WordsOnly, keywordExtractor);

                string atoken = atokenizer.FirstToken();
                while (atoken != null) {
                    docIndex.AddWordToDoc(document.Id, atoken, 10);
                    atoken = atokenizer.NextToken();
                }

                if (document.Text != null) {
                    atokenizer = new TextTokenizer(document.Text, TextFormats.WordsOnly, keywordExtractor);
                    atoken = atokenizer.FirstToken();
                    while (atoken != null) {
                        docIndex.AddWordToDoc(document.Id, atoken);
                        atoken = atokenizer.NextToken();
                    }
                }
            }

            docIndex.Recalculate();

            return docIndex;
        }
    }

    public class Document {
        public string Id;

        public string Title;

        public string Text;
    }

    public class Question {
        public string Title;

        public string Details;
    }
}
