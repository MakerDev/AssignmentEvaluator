using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssignmentEvaluator.Services.Exceptions
{

    [Serializable]
    public class NoAnswerFileFoundException : Exception
    {
        public NoAnswerFileFoundException() { }
        public NoAnswerFileFoundException(string message) : base(message) { }
        public NoAnswerFileFoundException(string message, Exception inner) : base(message, inner) { }
        protected NoAnswerFileFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
