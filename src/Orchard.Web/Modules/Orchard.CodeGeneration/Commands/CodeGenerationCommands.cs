using System;
using System.IO;
using System.Linq;
using Microsoft.AspNet.Hosting;
using Orchard.Environment.Commands;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Services;

namespace Orchard.CodeGeneration.Commands
{
    public class CodeGenerationCommands : DefaultOrchardCommandHandler
    {
        private readonly IExtensionManager _extensionManager;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IClock _clock;

        private const string SolutionDirectoryModules = "90030E85-0C4F-456F-B879-443E8A3F220D";
        private const string SolutionDirectoryThemes = "AF3BADEA-CC93-4116-B3C1-F8D074EBEE4F";

        private static readonly string[] _themeDirectories = {
            "", "Assets", "Content", "Styles", "Scripts", "Views"
        };
        private static readonly string[] _moduleDirectories = {
            "", "Assets", "Controllers", "Views", "Models", "Scripts", "Styles"
        };

        private const string ModuleName = "CodeGeneration";
        private static string _codeGenTemplatePath;
        private static string _orchardWebProj;
        
        public CodeGenerationCommands(IExtensionManager extensionManager, IHostingEnvironment hostingEnvironment, IClock clock)
        {
            _extensionManager = extensionManager;
            _hostingEnvironment = hostingEnvironment;
            _clock = clock;

            _codeGenTemplatePath = _hostingEnvironment.MapPath("../Modules/Orchard." + ModuleName + "/CodeGenerationTemplates/");
            _orchardWebProj = _hostingEnvironment.MapPath("../Orchard.Web.csproj");
        
            // Default is to include in the solution when generating modules / themes
            IncludeInSolution = true;
        }

        [OrchardSwitch]
        public bool IncludeInSolution { get; set; }

        [OrchardSwitch]
        public string BasedOn { get; set; }

        [CommandHelp("codegen module <module-name> [/IncludeInSolution:true|false]\r\n\t" + "Create a new Orchard module")]
        [CommandName("codegen module")]
        [OrchardSwitches("IncludeInSolution")]
        public void CreateModule(string moduleName)
        {
            Context.Output.WriteLine(T("Creating Module {0}", moduleName));

            if (_extensionManager.AvailableExtensions().Any(extension => String.Equals(moduleName, extension.Name, StringComparison.OrdinalIgnoreCase)))
            {
                Context.Output.WriteLine(T("Creating Module {0} failed: a module of the same name already exists", moduleName));
                return;
            }

            IntegrateModule(moduleName);
            Context.Output.WriteLine(T("Module {0} created successfully", moduleName));
        }

        [CommandName("codegen theme")]
        [CommandHelp("codegen theme <theme-name> [/IncludeInSolution:true|false][/BasedOn:<theme-name>]\r\n\tCreate a new Orchard theme")]
        [OrchardSwitches("IncludeInSolution,BasedOn")]
        public void CreateTheme(string themeName)
        {
            Context.Output.WriteLine(T("Creating Theme {0}", themeName));
            if (_extensionManager.AvailableExtensions().Any(extension => String.Equals(themeName, extension.Id, StringComparison.OrdinalIgnoreCase)))
            {
                Context.Output.WriteLine(T("Creating Theme {0} failed: an extention of the same name already exists", themeName));
                return;
            }

            if (!string.IsNullOrEmpty(BasedOn))
            {
                if (!_extensionManager.AvailableExtensions().Any(extension =>
                    string.Equals(extension.ExtensionType, DefaultExtensionTypes.Theme, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(BasedOn, extension.Id, StringComparison.OrdinalIgnoreCase)))
                {
                    Context.Output.WriteLine(T("Creating Theme {0} failed: base theme named {1} was not found.", themeName, BasedOn));
                    return;
                }
            }
            IntegrateTheme(themeName, BasedOn);
            Context.Output.WriteLine(T("Theme {0} created successfully", themeName));
        }

        private void IntegrateModule(string moduleName)
        {
            string projectGuid = Guid.NewGuid().ToString().ToUpper();

            CreateModuleFromTemplates(moduleName, projectGuid);
            // The string searches in solution/project files can be made aware of comment lines.
            if (IncludeInSolution)
            {
                AddToSolution(Context.Output, moduleName, projectGuid, "Modules", SolutionDirectoryModules);
            }
        }

        private void IntegrateTheme(string themeName, string baseTheme)
        {
            CreateThemeFromTemplates(Context.Output,
                themeName,
                baseTheme,
                Guid.NewGuid().ToString().ToUpper(),
                IncludeInSolution);
        }

        private void CreateModuleFromTemplates(string moduleName, string projectGuid)
        {
            string modulePath = _hostingEnvironment.MapPath("../Modules/" + moduleName + "/");

            foreach (var folder in _moduleDirectories)
            {
                Directory.CreateDirectory(modulePath + folder);
            }

            File.WriteAllText(modulePath + "Assets.json", File.ReadAllText(_codeGenTemplatePath + "ModuleAssetsJson.txt"));
            File.WriteAllText(modulePath + "Assets\\Styles.less", File.ReadAllText(_codeGenTemplatePath + "ModuleStylesLess.txt"));
            File.WriteAllText(modulePath + "Styles\\Styles.css", File.ReadAllText(_codeGenTemplatePath + "ModuleStylesCss.txt"));
            File.WriteAllText(modulePath + "Styles\\Styles.min.css", File.ReadAllText(_codeGenTemplatePath + "ModuleStylesMinCss.txt"));

            string templateText = File.ReadAllText(_codeGenTemplatePath + "ModuleManifest.txt");
            templateText = templateText.Replace("$$ModuleName$$", moduleName);
            File.WriteAllText(modulePath + "Module.txt", templateText, System.Text.Encoding.UTF8);
            File.WriteAllText(modulePath + "project.json", File.ReadAllText(_codeGenTemplatePath + "ModuleProjectJson.txt"));
            File.WriteAllText(modulePath + moduleName + ".xproj", CreateXProject(moduleName, projectGuid));
        }

        private static string CreateXProject(string projectName, string projectGuid)
        {
            string text = File.ReadAllText(_codeGenTemplatePath + "\\ModuleXProj.txt");
            text = text.Replace("$$ModuleName$$", projectName);
            text = text.Replace("$$ModuleProjectGuid$$", projectGuid);
            return text;
        }

        private void CreateThemeFromTemplates(TextWriter output, string themeName, string baseTheme, string projectGuid, bool includeInSolution)
        {
            var themePath = _hostingEnvironment.MapPath("../Themes/" + themeName + "/");

            // create directories
            foreach (var folderName in _themeDirectories)
            {
                var folder = themePath + folderName;
                Directory.CreateDirectory(folder);
            }

            File.WriteAllText(themePath + "Assets.json", File.ReadAllText(_codeGenTemplatePath + "ModuleAssetsJson.txt"));
            File.WriteAllText(themePath + "Assets\\Styles.less", File.ReadAllText(_codeGenTemplatePath + "ModuleStylesLess.txt"));
            File.WriteAllText(themePath + "Styles\\Styles.css", File.ReadAllText(_codeGenTemplatePath + "ModuleStylesCss.txt"));
            File.WriteAllText(themePath + "Styles\\Styles.min.css", File.ReadAllText(_codeGenTemplatePath + "ModuleStylesMinCss.txt"));
            var templateText = File.ReadAllText(_codeGenTemplatePath + "\\ThemeManifest.txt").Replace("$$ThemeName$$", themeName);
            templateText = string.IsNullOrEmpty(baseTheme) ? templateText.Replace("BaseTheme: $$BaseTheme$$\r\n", "") : templateText.Replace("$$BaseTheme$$", baseTheme);
            File.WriteAllText(themePath + "Theme.txt", templateText);
            File.WriteAllText(themePath + "project.json", File.ReadAllText(_codeGenTemplatePath + "ThemeProjectJson.txt"));
            File.WriteAllText(themePath + "_ViewImports.cshtml", File.ReadAllText(_codeGenTemplatePath + "ThemeViewImports.txt"));
            
            //TODO: Theme.png and placement.info not used in Orchard 2.x themes currently, will enable when supported
            //File.WriteAllBytes(themePath + "Theme.png", File.ReadAllBytes(_codeGenTemplatePath + "Theme.png"));
            //File.WriteAllText(themePath + "Placement.info", File.ReadAllText(_codeGenTemplatePath + "Placement.info"));
            
            // create new xproj for the theme
            if (projectGuid != null)
            {
                string projectText = CreateXProject(themeName, projectGuid);
                File.WriteAllText(themePath + "\\" + themeName + ".xproj", projectText);
            }

            if (includeInSolution)
            {
                AddToSolution(output, themeName, projectGuid, "Themes", SolutionDirectoryThemes);
            }
        }

        private void AddToSolution(TextWriter output, string projectName, string projectGuid, string containingFolder, string solutionFolderGuid)
        {
            if (!string.IsNullOrEmpty(projectGuid))
            {
                var solutionPath = Directory.GetParent(_orchardWebProj).Parent.Parent.FullName + "\\Orchard.sln";
                if (File.Exists(solutionPath))
                {
                    var projectReference = string.Format("EndProject\r\nProject(\"{{8BB2217D-0F2D-49D1-97BC-3654ED321F3B}}\") = \"{0}\", \"src\\Orchard.Web\\{2}\\{0}\\{0}.xproj\", \"{{{1}}}\"\r\n", projectName, projectGuid, containingFolder);
                    var projectConfiguationPlatforms = string.Format("GlobalSection(ProjectConfigurationPlatforms) = postSolution\r\n\t\t{{{0}}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU\r\n\t\t{{{0}}}.Debug|Any CPU.Build.0 = Debug|Any CPU\r\n\t\t{{{0}}}.Release|Any CPU.ActiveCfg = Release|Any CPU\r\n\t\t{{{0}}}.Release|Any CPU.Build.0 = Release|Any CPU\r\n", projectGuid);
                    var solutionText = File.ReadAllText(solutionPath);
                    solutionText = solutionText.Insert(solutionText.LastIndexOf("EndProject\r\n", StringComparison.Ordinal), projectReference).Replace("GlobalSection(ProjectConfigurationPlatforms) = postSolution\r\n", projectConfiguationPlatforms);
                    solutionText = solutionText.Insert(solutionText.LastIndexOf("EndGlobalSection", StringComparison.Ordinal), "\t{" + projectGuid + "} = {" + solutionFolderGuid + "}\r\n\t");
                    File.WriteAllText(solutionPath, solutionText);
                    TouchSolution(output);
                }
            }
        }

        private void TouchSolution(TextWriter output)
        {
            string rootWebProjectPath = _hostingEnvironment.MapPath("../Orchard.Web.csproj");
            string solutionPath = Directory.GetParent(rootWebProjectPath).Parent.Parent.FullName + "\\Orchard.sln";
            if (!File.Exists(solutionPath))
            {
                output.WriteLine(T("Warning: Solution file could not be found at {0}", solutionPath));
                return;
            }

            try
            {
                File.SetLastWriteTime(solutionPath, _clock.UtcNow.DateTime);
            }
            catch
            {
                output.WriteLine(T("An unexpected error occured while trying to refresh the Visual Studio solution. Please reload it."));
            }
        }
    }
}