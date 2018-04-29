using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace StackExchange
{
    public static class StringExtensions
    {
        private static Regex _blockQuoteRegex = new Regex("<blockquote>.+?</blockquote>", RegexOptions.Singleline);
        private static Regex _codeRegex = new Regex("<code>.+?</code>", RegexOptions.Singleline);
        private static Regex _tagsRegex = new Regex("(<.*?>\\s*)+", RegexOptions.Singleline);

        public static string GetInnerText(this string html) {
            var result = _blockQuoteRegex.Replace(html, "__BLOCKQUOTE");
            result = _codeRegex.Replace(result, "__CODE");
            return _tagsRegex.Replace(result, " ").Trim();
        }
    }
}
