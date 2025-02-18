using System;

namespace CodeLineCounter.Exceptions
{
    public class InvalidSelectionException : Exception

    {
        public InvalidSelectionException(int solutionCount) : base($"'Selection must be between 1 and {solutionCount}.'")
        {
        }
    }
}