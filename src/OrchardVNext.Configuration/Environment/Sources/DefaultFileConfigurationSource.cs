using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Framework.Configuration;

namespace OrchardVNext.Configuration.Environment.Sources {
    public class DefaultFileConfigurationSource : ConfigurationSource {
        public const char Separator = ':';
        public const string EmptyValue = "null";
        public const char ThemesSeparator = ';';

        public DefaultFileConfigurationSource(string path)
            : this(path, optional: false) {
        }

        public DefaultFileConfigurationSource(string path, bool optional) {
            if (string.IsNullOrEmpty(path)) {
                throw new ArgumentException("Invalid Filepath", nameof(path));
            }

            Optional = optional;
            Path = path;
        }

        /// <summary>
        /// Gets a value that determines if this instance of <see cref="DefaultFileConfigurationSource"/> is optional.
        /// </summary>
        public bool Optional { get; }

        /// <summary>
        /// The absolute path of the file backing this instance of <see cref="DefaultFileConfigurationSource"/>.
        /// </summary>
        public string Path { get; }

        public override void Load() {
            if (!File.Exists(Path)) {
                if (Optional) {
                    Data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                }
                else {
                    throw new FileNotFoundException(string.Format("File not found: ", Path), Path);
                }
            }
            else {
                using (var stream = new FileStream(Path, FileMode.Open, FileAccess.Read)) {
                    Load(stream);
                }
            }
        }

        internal void Load(Stream stream) {
            var data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            using (var reader = new StreamReader(stream)) {
                while (reader.Peek() != -1) {
                    var rawLine = reader.ReadLine();
                    var line = rawLine.Trim();

                    // Ignore blank lines
                    if (string.IsNullOrWhiteSpace(line)) {
                        continue;
                    }
                    // Ignore comments
                    if (line[0] == ';' || line[0] == '#' || line[0] == '/') {
                        continue;
                    }

                    var separatorIndex = line.IndexOf(Separator);
                    if (separatorIndex == -1) {
                        continue;
                    }
                    string key = line.Substring(0, separatorIndex).Trim();
                    string value = line.Substring(separatorIndex + 1).Trim();

                    if (value.Equals(EmptyValue, StringComparison.OrdinalIgnoreCase)) {
                        continue;
                    }

                    data[key] = value;
                }
            }

            Data = data;
        }

        public virtual void Commit() {
            if (File.Exists(Path)) {
                File.Delete(Path);
            }

            if (!new FileInfo(Path).Directory.Exists)
                Directory.CreateDirectory(new FileInfo(Path).Directory.FullName);

            // TODO: Revisit to make fully atomic.
            // https://github.com/aspnet/Configuration/pull/147/files

            var newConfigFileStream = new FileStream(Path, FileMode.CreateNew);

            try {
                // Generate contents and write it to the newly created config file
                GenerateNewConfig(newConfigFileStream);
            }
            catch {
                newConfigFileStream.Dispose();

                // The operation should be atomic because we don't want a corrupted config file
                // So we roll back if the operation fails
                if (File.Exists(Path)) {
                    File.Delete(Path);
                }

                // Rethrow the exception
                throw;
            }
            finally {
                newConfigFileStream.Dispose();
            }

            return;
        }
        

        internal void GenerateNewConfig(Stream outputStream) {
            var outputWriter = new StreamWriter(outputStream);

            foreach (var entry in Data) {
                outputWriter.WriteLine("{0}: {1}", entry.Key, (entry.Value ?? EmptyValue));
            }

            outputWriter.Flush();
        }
    }
}
