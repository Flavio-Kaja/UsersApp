namespace UserService.Exceptions
{
    using System;
    using System.Globalization;

    public class ForbiddenAccessException : Exception
    {
        public ForbiddenAccessException() : base() { }
    }
}