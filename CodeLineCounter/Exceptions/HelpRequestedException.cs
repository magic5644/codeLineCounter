using System;
using CodeLineCounter.Models;

namespace CodeLineCounter.Exceptions
{
    public class HelpRequestedException : Exception

    {
        public HelpRequestedException() : base($"Usage: CodeLineCounter.exe [-verbose] [-d <directory_path>] [-output <output_path>] [-help, -h] (-format <csv, json>)'")
        {
        }
    }
}