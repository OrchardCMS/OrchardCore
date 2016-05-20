// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.DotNet.Cli.CommandLine;
using Microsoft.DotNet.Cli.Compiler.Common;
using static Microsoft.DotNet.Cli.Compiler.Common.AssemblyInfoOptions;

namespace Microsoft.DotNet.Tools.Compiler
{
    internal class AssemblyInfoOptionsCommandLine
    {
        public CommandOption VersionOption { get; set; }
        public CommandOption TitleOption { get; set; }
        public CommandOption DescriptionOption { get; set; }
        public CommandOption CopyrightOption { get; set; }
        public CommandOption NeutralCultureOption { get; set; }
        public CommandOption CultureOption { get; set; }
        public CommandOption InformationalVersionOption { get; set; }
        public CommandOption FileVersionOption { get; set; }
        public CommandOption TargetFrameworkOption { get; set; }

        public static AssemblyInfoOptionsCommandLine AddOptions(CommandLineApplication app)
        {
            AssemblyInfoOptionsCommandLine commandLineOptions = new AssemblyInfoOptionsCommandLine();

            commandLineOptions.VersionOption = AddOption(app, AssemblyVersionOptionName, "Assembly version");

            commandLineOptions.TitleOption = AddOption(app, TitleOptionName, "Assembly title");

            commandLineOptions.DescriptionOption = AddOption(app, DescriptionOptionName, "Assembly description");

            commandLineOptions.CopyrightOption = AddOption(app, CopyrightOptionName, "Assembly copyright");

            commandLineOptions.NeutralCultureOption = AddOption(app, NeutralCultureOptionName, "Assembly neutral culture");

            commandLineOptions.CultureOption = AddOption(app, CultureOptionName, "Assembly culture");

            commandLineOptions.InformationalVersionOption = AddOption(app, InformationalVersionOptionName, "Assembly informational version");

            commandLineOptions.FileVersionOption = AddOption(app, AssemblyFileVersionOptionName, "Assembly file version");

            commandLineOptions.TargetFrameworkOption = AddOption(app, TargetFrameworkOptionName, "Assembly target framework");

            return commandLineOptions;
        }

        private static CommandOption AddOption(CommandLineApplication app, string optionName, string description)
        {
            return app.Option($"--{optionName} <arg>", description, CommandOptionType.SingleValue);
        }

        public AssemblyInfoOptions GetOptionValues()
        {
            return new AssemblyInfoOptions()
            {
                AssemblyVersion = UnescapeNewlines(VersionOption.Value()),
                Title = UnescapeNewlines(TitleOption.Value()),
                Description = UnescapeNewlines(DescriptionOption.Value()),
                Copyright = UnescapeNewlines(CopyrightOption.Value()),
                NeutralLanguage = UnescapeNewlines(NeutralCultureOption.Value()),
                Culture = UnescapeNewlines(CultureOption.Value()),
                InformationalVersion = UnescapeNewlines(InformationalVersionOption.Value()),
                AssemblyFileVersion = UnescapeNewlines(FileVersionOption.Value()),
                TargetFramework = UnescapeNewlines(TargetFrameworkOption.Value()),
            };
        }

        private static string UnescapeNewlines(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            return text.Replace("\\r", "\r").Replace("\\n", "\n");
        }
    }
}