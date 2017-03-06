using System;
using System.IO;

namespace Orchard.Hosting
{
    public class OrchardConsoleLogger
    {
        private readonly TextReader _input;
        private readonly TextWriter _output;

        public OrchardConsoleLogger(
            TextReader input,
            TextWriter output)
        {
            _input = input;
            _output = output;
        }

        public void LogInfo(string format, params object[] args)
        {
            _output.Write("{0}: ", DateTime.Now);
            _output.WriteLine(format, args);
        }

        public void LogError(string format, params object[] args)
        {
            _output.Write("{0}: ", DateTime.Now);
            _output.WriteLine(format, args);
        }
    }
}