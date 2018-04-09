using System;

namespace Korzh.DeepThought
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2) {
                Console.WriteLine("Using: deepth <command> <KB_ID> <XML file> [other options]");
                return;
            }

            var command = args[0];
            var kbId = args[1];
            var xmlFileName = args[2];

            if (command == "train") {
            }
            else if (command == "test") {

            }

        }

    //    private static void AddArticleToIndex(DocBaseIndex docIndex, string docId, string title, string content, string[] tags) {
    //        var keywordExtractor = new KeywordExtractor("en");

    //        docIndex.AddDocument(docId);

    //        docIndex.AddArticleTags(tags);

    //        var tokenizer = new TextTokenizer(title, TextFormats.WordsOnly, keywordExtractor);

    //        string token = tokenizer.FirstToken();
    //        while (token != null) {
    //            docIndex.AddWordToDoc(docId, token, 10);
    //            docIndex.AddWordToTags(tags, token, 5);
    //            token = tokenizer.NextToken();
    //        }

    //        tokenizer = new TextTokenizer(content, TextFormats.WordsOnly, keywordExtractor);
    //        token = tokenizer.FirstToken();
    //        while (token != null) {
    //            docIndex.AddWordToDoc(docId, token);
    //            docIndex.AddWordToTags(tags, token);
    //            token = tokenizer.NextToken();
    //        }
    //    }

    }
}
