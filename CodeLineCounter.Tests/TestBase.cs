using System;
using System.IO;
using System.Threading;
using Xunit;

namespace CodeLineCounter.Tests
{
    public abstract class TestBase : IDisposable
    {
        private static readonly object _consoleLock = new object();
        private readonly ThreadLocal<StringWriter> _stringWriter = new ThreadLocal<StringWriter>(() => new StringWriter());
        private readonly ThreadLocal<StringReader> _stringReader = new ThreadLocal<StringReader>(() => new StringReader(string.Empty));
        private readonly ThreadLocal<TextWriter> _originalConsoleOut = new ThreadLocal<TextWriter>();
        private readonly ThreadLocal<TextReader> _originalConsoleIn = new ThreadLocal<TextReader>();

        private bool _disposed;

        protected TestBase()
        {

        }

        protected void initialization()
        {
            lock (_consoleLock)
            {
                _originalConsoleOut.Value = Console.Out;
                _originalConsoleIn.Value = Console.In;
            }

        }

        protected void RedirectConsoleInputOutput()
        {
            lock (_consoleLock)
            {
                if (_stringWriter.Value != null)
                {
                    Console.SetOut(_stringWriter.Value);
                }
                if (_stringReader.Value != null)
                {
                    Console.SetIn(_stringReader.Value);
                }
            }
        }

        protected string GetConsoleOutput()
        {
            lock (_consoleLock)
            {
                if (_stringWriter.Value != null)
                {
                    return _stringWriter.Value.ToString();
                }
                return string.Empty;
            }
        }

        protected void SetConsoleInput(string input)
        {
            lock (_consoleLock)
            {
                _stringReader.Value = new StringReader(input);
            }
        }

        protected void ResetConsoleInputOutput()
        {
            lock (_consoleLock)
            {
                if (_originalConsoleOut != null && _originalConsoleOut.Value != null)
                {
                    Console.SetOut(_originalConsoleOut.Value);
                }
                if (_originalConsoleIn != null && _originalConsoleIn.Value != null)
                {
                    Console.SetIn(_originalConsoleIn.Value);
                }
                if (_stringWriter.Value != null)
                {
                    _stringWriter.Value.GetStringBuilder().Clear(); // Clear the StringWriter's buffer
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    ResetConsoleInputOutput();
                    _stringWriter.Value?.Dispose();
                    _stringReader.Value?.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~TestBase()
        {
            Dispose(false);
        }
    }
}