using System;
using System.Collections.Generic;
using System.Text;

namespace Korzh.NLP
{

    public interface ITextProcessingServiceProvider {
        IStopWordFilter GetStopWordFilter();
        IStemmer GetStemmer();
    }

    public class TextProcessingServiceProvider : ITextProcessingServiceProvider {
        private string _lang;

        public TextProcessingServiceProvider(string lang) {
            _lang = lang;
        }

        public IStemmer GetStemmer() {
            if (_lang == "en") {
                return new EnglishStemmer();
            }
            else
                throw new NotImplementedException();
        }

        public IStopWordFilter GetStopWordFilter() {
            if (_lang == "en") {
                return new StopWordFilter(Dictionaries.EnglishStopWords);
            }
            else
                throw new NotImplementedException();
        }

    }
}
