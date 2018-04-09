using System;
using System.Collections.Generic;
using System.Text;

namespace Korzh.NLP
{
    public class KeywordExtractor : ITextFilterMapper {

        private readonly IStopWordFilter _stopWordFilter;

        private readonly IStemmer _wordStemmer;

        private readonly Func<string, bool> _filter;
        private readonly Func<string, string> _mapper;


        public KeywordExtractor(string lang) {

            var services = new TextProcessingServiceProvider(lang);

            _stopWordFilter = services.GetStopWordFilter();

            _wordStemmer = services.GetStemmer();

            _filter = (word) => {
                return !_stopWordFilter.IsStopWord(word);
            };

            _mapper = (word) => {
                return _wordStemmer.Stem(word);
            };

        }

        public Func<string, bool> Filter => _filter;

        public Func<string, string> Map => _mapper;
    }
}
