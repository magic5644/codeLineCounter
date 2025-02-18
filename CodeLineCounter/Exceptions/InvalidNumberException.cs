using System;

namespace CodeLineCounter.Exceptions
{
    public class InvalidNumberException : Exception

    {
        public InvalidNumberException() : base($"'Invalid input. Please enter a numeric value.'")
        {
        }
    }
}