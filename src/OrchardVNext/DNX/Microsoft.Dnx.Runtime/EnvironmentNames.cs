// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Dnx.Runtime {
    internal static class EnvironmentNames {
        public const string Packages = "NUGET_PACKAGES";
        public const string DnxPackages = "DNX_PACKAGES";
        public const string PackagesCache = "DNX_PACKAGES_CACHE";
        public const string Servicing = "DNX_SERVICING";
        public const string Trace = "DNX_TRACE";
        public const string CompilationServerPort = "DNX_COMPILATION_SERVER_PORT";
        public const string Home = "DNX_HOME";
        public const string GlobalPath = "DNX_GLOBAL_PATH";
        public const string AppBase = "DNX_APPBASE";
        public const string Framework = "DNX_FRAMEWORK";
        public const string Configuration = "DNX_CONFIGURATION";
        public const string ConsoleHost = "DNX_CONSOLE_HOST";
        public const string DefaultLib = "DNX_DEFAULT_LIB";
        public const string BuildKeyFile = "DNX_BUILD_KEY_FILE";
        public const string BuildDelaySign = "DNX_BUILD_DELAY_SIGN";
        public const string Sources = "DNX_SOURCES";
        public const string DnxIsWindows = "DNX_IS_WINDOWS";
        public const string AspNetLoaderPath = "DNX_ASPNET_LOADER_PATH";
    }
}