using Microsoft.Framework.PackageManager;
using System;

namespace OrchardVNext {
    public class FakeReport : IReport {
        public void WriteLine(string message) {
            Console.WriteLine(message);
        }
    }
}