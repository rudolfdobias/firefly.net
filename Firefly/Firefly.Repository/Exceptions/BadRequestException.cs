using System;

namespace Firefly.Repository.Exceptions
{
    public class BadRequestException : InvalidOperationException
    {
        private readonly Errors _code;
        private readonly object _data;

        public BadRequestException(string message)
            : base(message)
        {
            _code = Errors.InvalidOperation;
        }

        public BadRequestException(string message, Errors code)
            : base(message)
        {
            _code = code;
        }

        public BadRequestException(string message, Errors code, object data)
            : base(message)
        {
            _code = code;
            _data = data;
        }

        public object GetData()
        {
            return _data;
        }

        public Errors GetCode()
        {
            return _code;
        }
    }
}