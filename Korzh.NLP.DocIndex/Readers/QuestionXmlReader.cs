using System;
using System.Xml;
using System.Text;
using System.IO;

namespace Korzh.NLP.DocIndex
{
    public static class QuestionXmlReader {

        public static void ReadFile(string filePath, Func<Question, bool> callback) {
            if (!File.Exists(filePath)) {
                throw new DocBaseError("File does not exists: " + filePath);
            }

            if (callback == null) {
                throw new DocBaseError("Callback must not be null");
            }

            using (var reader = XmlReader.Create(new FileStream(filePath, FileMode.Open))) {
                Question question = null;
                while (reader.Read()) {
                    if (reader.NodeType == XmlNodeType.Element) {
                        if (reader.LocalName == "Question") {
                            question = new Question();

                            question.Id = reader.GetAttribute("id");
                            question.Title = reader.GetAttribute("title");
                        }
                        else if (reader.LocalName == "Content") {
                            question.Content = reader.ReadElementContentAsString();
                        }
                        else if (reader.LocalName == "DocId") {
                            question.DocId = reader.ReadElementContentAsString();
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "Question") {
                        if (!callback(question)) {
                            break;
                        }
                    }
                }
            }
        }
    }
}
