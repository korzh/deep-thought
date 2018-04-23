using System;
using System.Runtime.Serialization;

namespace Korzh.NLP.DocIndex {
    [Serializable]
    internal class DocBaseError : Exception {
        public DocBaseError() {
        }

        public DocBaseError(string message) : base(message) {
        }

        public DocBaseError(string message, Exception innerException) : base(message, innerException) {
        }

        protected DocBaseError(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}