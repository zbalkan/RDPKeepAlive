using System;

namespace RDPKeepAlive
{
    public class KeepAliveException : Exception
    {
        public KeepAliveException() : base()
        {
        }

        public KeepAliveException(string? message) : base(message)
        {
        }

        public KeepAliveException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}