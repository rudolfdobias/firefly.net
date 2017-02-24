using System;

namespace Firefly.Repository.Exceptions
{
    public class ForbiddenException : InvalidOperationException
    {
        public Errors Code { get; set; } = Errors.Forbidden;

        public ForbiddenException(string message)
            : base(message)
        {
        }

        public ForbiddenException(string message, Errors code)
            : base(message)
        {
            Code = code;
        }
    }
}