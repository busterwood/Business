using System;
using System.Runtime.Serialization;

namespace SampleTests
{
    [Serializable]
    internal class SomeBusinessException : Exception
    {
        public SomeBusinessException()
        {
        }

        public SomeBusinessException(string message) : base(message)
        {
        }

        public SomeBusinessException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SomeBusinessException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}