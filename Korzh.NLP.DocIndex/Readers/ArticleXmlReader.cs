using System;
using System.Xml;
using System.Text;
using System.IO;

namespace Korzh.NLP.DocIndex
{
    public static class ArticleXmlReader {

        public static void ReadFile(string filePath, Func<Document, bool> callback) {
            if (!File.Exists(filePath)) {
                throw new DocBaseError("File does not exists: " + filePath);
            }

            if (callback == null) {
                throw new DocBaseError("Callback must not be null");
            }

            using (var reader = XmlReader.Create(new FileStream(filePath, FileMode.Open))) {
                Document document = null;
                while (reader.Read()) {
                    if (reader.NodeType == XmlNodeType.Element) {
                        if (reader.LocalName == "Article") {
                            document = new Document();

                            document.Id = reader.GetAttribute("id");
                            document.Title = reader.GetAttribute("title");
                        }
                        else if (reader.LocalName == "Content") {
                            document.Text = reader.ReadElementContentAsString();
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "Article") {
                        if (!callback(document)) {
                            break;
                        }
                    }
                }
            }
        }
    }
}
