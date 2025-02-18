using System;
using CodeLineCounter.Models;

namespace CodeLineCounter.Exceptions
{
    public class InvalidExportFormatException : Exception

    {
        public InvalidExportFormatException(string formatString, Settings settings) : base($"Invalid format: {formatString}. Valid formats are: {string.Join(", ", Enum.GetNames<Utils.CoreUtils.ExportFormat>())}. Using default format {settings.Format}'")
        {
        }
    }
}