using System;
using System.IO;
using System.Linq;

using Korzh.NLP;
using Korzh.NLP.DocIndex;

namespace Korzh.DeepThought
{
    class Program
    {
        private static string _dataFolder = "data";

        static void Main(string[] args)
        {
            if (args.Length < 2) {
                Console.WriteLine("Using: deepth <command> <DocBase ID> <XML file> [other options]");
                return;
            }

            var command = args[0];
            var dbId = args[1];
            var xmlFileName = args[2];

            int maxCount = 0;
            if (args.Length > 3) {
                int.TryParse(args[3], out maxCount);
            }

            if (command == "init") {
                InitDb(dbId, xmlFileName, maxCount);
            }
            else if (command == "train") {
                Train(dbId, xmlFileName, maxCount);
            }
            else if (command == "test") {
                Test(dbId, xmlFileName, maxCount);
            }
#if DEBUG
            Console.ReadKey();
#endif
        }

        private static void InitDb(string dbId, string docsFileName, int maxCount = 0) {
            Console.WriteLine($"Initializing DocBase {dbId} with the documents from [{docsFileName}]...");

            var keywordExtractor = new KeywordExtractor("en");
            var builder = new DocIndexBuilder();
            var count = 0;
            ArticleXmlReader.ReadFile(docsFileName, document => {
                builder.AddDocument(document, keywordExtractor);
                count++;
                if (count % 500 == 0) {
                    Console.Write($"\r{count} documents and {builder.DocIndex.Words.Count} words processed.     ");
                }

                if (maxCount > 0 && count >= maxCount) {
                    return false;
                }
                return true;
            });

            Console.WriteLine();

            var docIndex = builder.DocIndex;

            Console.WriteLine($"Calculating TFIDF...");
            docIndex.Recalculate();

            var docBaseFolder = Path.Combine(_dataFolder, dbId);

            var wordsFilePath = Path.Combine(docBaseFolder, "vocabulary.txt");
            Console.WriteLine($"Dumping {dbId} vocabulary to [{wordsFilePath}]...");
            using (var dictFile = new StreamWriter(wordsFilePath)) {
                foreach (var we in docIndex.Words) {
                    var wordScore = we.Value.DocScores.Sum(wds => wds.TFIDF);
                    dictFile.WriteLine($"{we.Key}:{wordScore}");
                }
            }


            var docIndexRepo = new DocBaseIndexFileRepository(docBaseFolder);

            Console.WriteLine($"Saving the index...");
            docIndexRepo.SaveAsync(dbId, docIndex).Wait();

            Console.WriteLine($"Done! {count} documents were added to the index");
        }


        private static void Train(string dbId, string questionsFileName, int maxCount = 0) {
            Console.WriteLine($"Training DocBase {dbId} with the documents from [{questionsFileName}]...");

            var docBaseFolder = System.IO.Path.Combine(_dataFolder, dbId);
            var docIndexRepo = new DocBaseIndexFileRepository(docBaseFolder);

            var docIndex = docIndexRepo.LoadAsync(dbId).Result;

            var keywordExtractor = new KeywordExtractor("en");

            var builder = new DocIndexBuilder(docIndex);
            var count = 0;
            QuestionXmlReader.ReadFile(questionsFileName, question => {
                builder.AddQuestion(question, keywordExtractor);
                count++;
                if (count % 500 == 0) {
                    Console.Write($"\r{count} questions processed.  ");
                }
                if (maxCount > 0 && count >= maxCount) {
                    return false;
                }
                return true;
            });

            Console.WriteLine($"Recalculating TFIDF...");
            docIndex.Recalculate();

            Console.WriteLine($"Saving the index...");
            docIndexRepo.SaveAsync(dbId, docIndex).Wait();

            Console.WriteLine($"Done! {count} questions were added to the index");
        }


        private static void Test(string dbId, string xmlFileName, int maxCount = 0) {
            Console.WriteLine($"Testing DocBase {dbId} with the questions from [{xmlFileName}]...");

            var docBaseFolder = System.IO.Path.Combine(_dataFolder, dbId);
            var docIndexRepo = new DocBaseIndexFileRepository(docBaseFolder);

            var docIndex = docIndexRepo.LoadAsync(dbId).Result;

            var keywordExtractor = new KeywordExtractor("en");

            var classifier = new SmartTextClassifier("en", docIndex);

            var count = 0;
            double TP = 0, FP = 0, FN = 0, TN = 0;
            QuestionXmlReader.ReadFile(xmlFileName, question => {
                var docPredicts = classifier.SuggestDocs(question.Title + "\n" + question.Content, 3);

                var mostRelevantPrediction = docPredicts.FirstOrDefault();

                if (question.DocId != null) {
                    bool foundInPredicts = docPredicts.FirstOrDefault(dp => dp.DocId == question.DocId) != null;
                    if (foundInPredicts) {
                        TP++;
                    }
                    else {
                        FP++;
                    }
                }
                else {
                    if (mostRelevantPrediction != null) {
                        FN++;
                    }
                    else {
                        TN++;
                    }
                }
                count++;
                if (count % 100 == 0) {
                    Console.Write($"\r{count} questions processed.  ");
                }

                if (maxCount > 0 && count >= maxCount) {
                    return false;
                }
                return true;
            });

            double precision = TP / (TP + FP);
            double recall = TP / (TP + FN);

            Console.WriteLine();
            Console.WriteLine($"Done!");
            Console.WriteLine($"TP:{TP}, FP:{FP}, TN: {TN}, FN:{FN}");
            Console.WriteLine($"Precision:{precision}, Recall:{recall}");
        }

    }
}
