using System;

namespace N2.Http.Exceptions
{
    public class AlreadyExistsException : Exception
    {
        public AlreadyExistsException() : base("The name used for creating an object, is already in use")
        {

        }

        public AlreadyExistsException(string message) : base(message)
        {

        }

        public AlreadyExistsException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}

