// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.DotNet.Cli.CommandLine;
using Microsoft.DotNet.ProjectModel;
using static Microsoft.DotNet.Cli.Compiler.Common.CommonCompilerOptionsExtensions;

namespace Microsoft.DotNet.Tools.Compiler
{
    internal class CommonCompilerOptionsCommandLine
    {
        public CommandOption DefineOption { get; set; }
        public CommandOption SuppressWarningOption { get; set; }
        public CommandOption LanguageVersionOption { get; set; }
        public CommandOption PlatformOption { get; set; }
        public CommandOption AllowUnsafeOption { get; set; }
        public CommandOption WarningsAsErrorsOption { get; set; }
        public CommandOption OptimizeOption { get; set; }
        public CommandOption KeyFileOption { get; set; }
        public CommandOption DelaySignOption { get; set; }
        public CommandOption PublicSignOption { get; set; }
        public CommandOption DebugTypeOption { get; set; }
        public CommandOption EmitEntryPointOption { get; set; }
        public CommandOption GenerateXmlDocumentationOption { get; set; }
        public CommandOption AdditionalArgumentsOption { get; set; }
        public CommandOption OutputNameOption { get; set; }

        public static CommonCompilerOptionsCommandLine AddOptions(CommandLineApplication app)
        {
            CommonCompilerOptionsCommandLine commandLineOptions = new CommonCompilerOptionsCommandLine();

            commandLineOptions.DefineOption =
                AddOption(app, DefineOptionName, "Preprocessor definitions", CommandOptionType.MultipleValue);

            commandLineOptions.SuppressWarningOption =
                AddOption(app, SuppressWarningOptionName, "Suppresses the specified warning", CommandOptionType.MultipleValue);

            commandLineOptions.LanguageVersionOption =
                AddOption(app, LanguageVersionOptionName, "The version of the language used to compile", CommandOptionType.SingleValue);

            commandLineOptions.PlatformOption =
                AddOption(app, PlatformOptionName, "The target platform", CommandOptionType.SingleValue);

            commandLineOptions.AllowUnsafeOption =
                AddOption(app, AllowUnsafeOptionName, "Allow unsafe code", CommandOptionType.BoolValue);

            commandLineOptions.WarningsAsErrorsOption =
                AddOption(app, WarningsAsErrorsOptionName, "Turn all warnings into errors", CommandOptionType.BoolValue);

            commandLineOptions.OptimizeOption =
                AddOption(app, OptimizeOptionName, "Enable compiler optimizations", CommandOptionType.BoolValue);

            commandLineOptions.KeyFileOption =
                AddOption(app, KeyFileOptionName, "Path to file containing the key to strong-name sign the output assembly", CommandOptionType.SingleValue);

            commandLineOptions.DelaySignOption =
                AddOption(app, DelaySignOptionName, "Delay-sign the output assembly", CommandOptionType.BoolValue);

            commandLineOptions.PublicSignOption =
                AddOption(app, PublicSignOptionName, "Public-sign the output assembly", CommandOptionType.BoolValue);

            commandLineOptions.DebugTypeOption =
                AddOption(app, DebugTypeOptionName, "The type of PDB to emit: portable or full", CommandOptionType.SingleValue);

            commandLineOptions.EmitEntryPointOption =
                AddOption(app, EmitEntryPointOptionName, "Output an executable console program", CommandOptionType.BoolValue);

            commandLineOptions.GenerateXmlDocumentationOption =
                AddOption(app, GenerateXmlDocumentationOptionName, "Generate XML documentation file", CommandOptionType.BoolValue);

            commandLineOptions.AdditionalArgumentsOption =
                AddOption(app, AdditionalArgumentsOptionName, "Pass the additional argument directly to the compiler", CommandOptionType.MultipleValue);

            commandLineOptions.OutputNameOption =
                AddOption(app, OutputNameOptionName, "Output assembly name", CommandOptionType.SingleValue);

            return commandLineOptions;
        }

        private static CommandOption AddOption(CommandLineApplication app, string optionName, string description, CommandOptionType optionType)
        {
            string argSuffix = optionType == CommandOptionType.MultipleValue ? "..." : null;
            string argString = optionType == CommandOptionType.BoolValue ? null : $" <arg>{argSuffix}";

            return app.Option($"--{optionName}{argString}", description, optionType);
        }

        public CommonCompilerOptions GetOptionValues()
        {
            return new CommonCompilerOptions()
            {
                Defines = DefineOption.Values,
                SuppressWarnings = SuppressWarningOption.Values,
                LanguageVersion = LanguageVersionOption.Value(),
                Platform = PlatformOption.Value(),
                AllowUnsafe = AllowUnsafeOption.BoolValue,
                WarningsAsErrors = WarningsAsErrorsOption.BoolValue,
                Optimize = OptimizeOption.BoolValue,
                KeyFile = KeyFileOption.Value(),
                DelaySign = DelaySignOption.BoolValue,
                PublicSign = PublicSignOption.BoolValue,
                DebugType = DebugTypeOption.Value(),
                EmitEntryPoint = EmitEntryPointOption.BoolValue,
                GenerateXmlDocumentation = GenerateXmlDocumentationOption.BoolValue,
                AdditionalArguments = AdditionalArgumentsOption.Values,
                OutputName = OutputNameOption.Value(),
            };
        }
    }
}