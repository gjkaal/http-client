using System;

namespace N2.Http.Exceptions
{

    public class PrincipalIsMissingException : Exception
    {
        public PrincipalIsMissingException() : base("The current thread does not have a principal assigned")
        {

        }

        public PrincipalIsMissingException(string message) : base(message)
        {

        }

        public PrincipalIsMissingException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}

