using System.Collections.Generic;

namespace OrchardCore.Hosting.ResponseFiles
{
    public class ResponseFiles
    {
        public IEnumerable<ResponseLine> ReadFiles(IEnumerable<string> filenames)
        {
            foreach (var filename in filenames)
            {
                foreach (var line in new ResponseFileReader().ReadLines(filename))
                {
                    yield return line;
                }
            }
        }
    }
}