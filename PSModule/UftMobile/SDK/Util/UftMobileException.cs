using System;
using System.Management.Automation;
using System.Runtime.Serialization;

namespace PSModule.UftMobile.SDK.Util
{
    [Serializable]
    public class UftMobileException : Exception
    {
        private readonly ErrorCategory _category;
        public UftMobileException(string message, ErrorCategory categ = ErrorCategory.NotSpecified) : base(message)
        {
            _category = categ;
        }
        protected UftMobileException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            // https://rules.sonarsource.com/csharp/RSPEC-3925
        }
        public ErrorCategory Category { get; }

    }
}
