using System;
using System.IO;
using System.Xml;

using CsvHelper;
using CsvHelper.Configuration;

namespace StackExchange
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Reading questions and answers...");
            ReadAnswers("QuestionsAnswers.csv", "sofdn-articles.xml", "qtemp.xml");
         
            Console.WriteLine("Reading duplicated questions...");        
            var count = ReadQuestions("QDuplicates.csv", "qtemp.xml", "sofdn-questions.xml");

            var trainCount = count / 5;

            SplitQuestionsXml("sofdn-questions.xml", "sofdn-questions-train.xml", "sofdn-questions-test.xml", trainCount);

            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("Done");

            
            //"QDuplicates.csv";
        }

        private static int ReadAnswers(string csvFilePath, string articlesFilePath, string questionsFilePath) {
            XmlWriterSettings xmlSettings = new XmlWriterSettings {
                Encoding = System.Text.Encoding.UTF8,
                OmitXmlDeclaration = true,
                Indent = true,
                CloseOutput = true
            };

            int count = 0;
            using (var textReader = new StreamReader(csvFilePath)) 
            using (var articlesXml = XmlWriter.Create(articlesFilePath, xmlSettings))
            using (var questionsXml = XmlWriter.Create(questionsFilePath, xmlSettings)) {
                articlesXml.WriteStartElement("Articles");
                questionsXml.WriteStartElement("Questions");
                var csv = new CsvReader( textReader );
                csv.Read();
                csv.ReadHeader();

                while (csv.Read()) {                    
                    var qa = csv.GetRecord<SofPostWithAnswer>();
                    articlesXml.WriteStartElement("Article");
                    articlesXml.WriteAttributeString("id", qa.AID);
                    articlesXml.WriteAttributeString("score", qa.AScore.ToString());
                    articlesXml.WriteAttributeString("title", qa.Title);
                    articlesXml.WriteElementString("Content", qa.ABody.GetInnerText());                    
                    articlesXml.WriteEndElement();

                    questionsXml.WriteStartElement("Question");
                    questionsXml.WriteAttributeString("id", qa.QID);
                    questionsXml.WriteAttributeString("score", qa.QScore.ToString());
                    questionsXml.WriteAttributeString("title", qa.Title);
                    questionsXml.WriteElementString("Content", qa.QBody.GetInnerText());  
                    questionsXml.WriteElementString("DocId", qa.AID);
                    questionsXml.WriteEndElement();
                    count++;
                }
                questionsXml.WriteEndElement();
                articlesXml.WriteEndElement();
            }
            return count;
        }

        private static int ReadQuestions(string csvFilePath, string questionsTempFilePath, string questionsFilePath) {
            XmlWriterSettings xmlSettings = new XmlWriterSettings {
                Encoding = System.Text.Encoding.UTF8,
                OmitXmlDeclaration = true,
                Indent = true,
                CloseOutput = true
            };

            int count = 0;

            using (var textReader = new StreamReader(csvFilePath)) 
            using (var qtXml = XmlReader.Create(questionsTempFilePath))
            using (var questionsXml = XmlWriter.Create(questionsFilePath, xmlSettings)) {
                questionsXml.WriteStartElement("Questions");

                //copy all previous questions
                Question question = new Question();
                bool insideQuestion = false;
                while (qtXml.Read()) {
                    if (qtXml.NodeType == XmlNodeType.Element) {
                        if (qtXml.LocalName == "Question") {
                            question.Id = qtXml.GetAttribute("id");
                            question.Title = qtXml.GetAttribute("title");
                            question.Score = qtXml.GetAttribute("score");
                            insideQuestion = true;
                        }
                        else if (insideQuestion && qtXml.LocalName == "Content") {
                            question.Content = qtXml.ReadElementContentAsString();
                        }       
                        else if (insideQuestion && qtXml.LocalName == "DocId") {
                            question.DocId = qtXml.ReadElementContentAsString();
                        }    
                    }
                    else if (qtXml.NodeType == XmlNodeType.EndElement) {
                        SaveQuestionToXml(questionsXml, question);    
                        insideQuestion = false;
                        count++;
                    }
                }

                var csv = new CsvReader( textReader );
                csv.Read();
                csv.ReadHeader();

                while (csv.Read()) {                    
                    var qd = csv.GetRecord<DuplicatedQuestion>();
                    question.Id = csv[0];
                    question.Score = csv[1];
                    question.Title = csv[2];
                    question.Content = csv[3];
                    question.DocId = csv[4];
                    SaveQuestionToXml(questionsXml, question);
                    count++;
                }
                questionsXml.WriteEndElement();
            }
            return count;
        }


        private static void SplitQuestionsXml(string xmlFilePath, string trainFilePath, string testFilePath, int trainNodeCount) {
            XmlWriterSettings xmlSettings = new XmlWriterSettings {
                Encoding = System.Text.Encoding.UTF8,
                OmitXmlDeclaration = true,
                Indent = true,
                CloseOutput = true
            };

            using (var srcXml = XmlReader.Create(xmlFilePath))
            using (var trainXml = XmlWriter.Create(trainFilePath, xmlSettings))
                using (var testXml = XmlWriter.Create(testFilePath, xmlSettings)) {
                trainXml.WriteStartElement("Questions");
                testXml.WriteStartElement("Questions");
                int N = 0;
                Question question = new Question();
                bool insideQuestion = false;
                while (srcXml.Read()) {
                    if (srcXml.NodeType == XmlNodeType.Element) {
                        if (srcXml.LocalName == "Question") {
                            question.Id = srcXml.GetAttribute("id");
                            question.Title = srcXml.GetAttribute("title");
                            question.Score = srcXml.GetAttribute("score");
                            insideQuestion = true;
                        }
                        else if (insideQuestion && srcXml.LocalName == "Content") {
                            question.Content = srcXml.ReadElementContentAsString();
                        }
                        else if (insideQuestion && srcXml.LocalName == "DocId") {
                            question.DocId = srcXml.ReadElementContentAsString();
                        }
                    }
                    else if (srcXml.NodeType == XmlNodeType.EndElement) {
                        var destXml = (N > trainNodeCount) ? trainXml : testXml;
                        SaveQuestionToXml(destXml, question);
                        insideQuestion = false;
                        N++;
                    }
                }
                trainXml.WriteEndElement();
                testXml.WriteEndElement();
            }
        }
        private static void SaveQuestionToXml(XmlWriter questionsXml, Question question) {
            questionsXml.WriteStartElement("Question");
            questionsXml.WriteAttributeString("id", question.Id);
            questionsXml.WriteAttributeString("score", question.Score);
            questionsXml.WriteAttributeString("title", question.Title);
            questionsXml.WriteElementString("Content", question.Content);  
            questionsXml.WriteElementString("DocId", question.DocId);
            questionsXml.WriteEndElement();
        }
    }

    internal class DuplicatedQuestion
    {
        public string QID { get; set; }
        public string QScore { get; set; }
        public string Title { get; set; } 
        public string QBody { get; set; }        
        public string RelatedPostId { get; set; }
    }

    internal class Question {
        public string Id;
        public string Title;
        public string Score;
        public string Content;
        public string DocId;
    }

    public class SofPostWithAnswer {
        public string QID { get; set; }
        public string QScore { get; set; }
        public string Title { get; set; }
        public string QBody { get; set; }
        public string AID { get; set; }
        public string AScore { get; set; }
        public string ABody { get; set; }
    }

    public sealed class SofPostWithAnswerClassMap : ClassMap<SofPostWithAnswer> {
        public SofPostWithAnswerClassMap() {
            AutoMap();
        }
    }

}
