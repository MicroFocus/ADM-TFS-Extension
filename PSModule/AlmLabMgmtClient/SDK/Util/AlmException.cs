using System;
using System.Management.Automation;
using System.Runtime.Serialization;

namespace PSModule.AlmLabMgmtClient.SDK.Util
{
    [Serializable]
    public class AlmException : Exception
    {
        private readonly ErrorCategory _category;
        public AlmException(string message, ErrorCategory categ = ErrorCategory.NotSpecified) : base(message)
        {
            _category = categ;
        }
        protected AlmException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            // https://rules.sonarsource.com/csharp/RSPEC-3925
        }
        public ErrorCategory Category { get; }

    }
}
