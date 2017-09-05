using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OrchardCore.Environment.Commands.Parameters;

namespace OrchardCore.Hosting.ResponseFiles
{
    public class ResponseLine
    {
        public string Filename { get; set; }
        public string LineText { get; set; }
        public int LineNumber { get; set; }
        public string[] Args { get; set; }
    }

    public class ResponseFileReader
    {
        public IEnumerable<ResponseLine> ReadLines(string filename)
        {
            using (var reader = File.OpenText(filename))
            {
                for (int i = 0; ; i++)
                {
                    string lineText = reader.ReadLine();
                    if (lineText == null)
                        yield break;

                    yield return new ResponseLine
                    {
                        Filename = filename,
                        LineText = lineText,
                        LineNumber = i,
                        Args = new CommandParser().Parse(lineText).ToArray()
                    };
                }
            }
        }
    }
}