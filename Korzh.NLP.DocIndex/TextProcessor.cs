using Korzh.NLP;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aistant.NLP
{
    public class TextProcessor
    {
        private KeywordExtractor _keywordExtractor;
        private string _text;
        
        public TextProcessor(string lang, string text) {
            _keywordExtractor = new KeywordExtractor(lang);
            _text = text;
        }

        public void Run(Action<string, int> nextTokenCallback, Action finishCallback = null) {
            var tokenizer = new TextTokenizer(_text, TextFormats.WordsOnly, _keywordExtractor);
            var tagsDict = new Dictionary<string, double>();

            int index = 0;
            string token = tokenizer.FirstToken();
            while (token != null) {
                nextTokenCallback(token, index);
                token = tokenizer.NextToken();
                index++;
            }

            finishCallback?.Invoke();
        }


        public IEnumerable<string> Tokenize() {
            var tokenizer = new TextTokenizer(_text, TextFormats.WordsOnly, _keywordExtractor);
            var tagsDict = new Dictionary<string, double>();

            string token = tokenizer.FirstToken();
            while (token != null) {
                yield return token;    
                token = tokenizer.NextToken();
            }
        }
    }
}
