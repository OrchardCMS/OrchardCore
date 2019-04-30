using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OrchardCore.Abstractions.Modules
{
    public interface IFileVersionHashProvider
    {
        string GetFileVersionHash(Stream fileStream);
    }
}
