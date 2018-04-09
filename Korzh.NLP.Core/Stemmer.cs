using System;
using System.Collections.Generic;
using System.Text;

namespace Korzh.NLP
{
    public interface IStemmer : Iveonik.Stemmers.IStemmer {
    }

    class EnglishStemmer : Iveonik.Stemmers.EnglishStemmer, IStemmer {

    }
}
