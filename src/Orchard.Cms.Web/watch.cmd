@echo off
set "application=%cd%"
set "modules=%application%\..\OrchardCore.Modules"
set "themes=%application%\..\OrchardCore.Themes"
set "targets=/t:CopyPackageRazorFiles"
set "props=/p:ApplicationDirectory=%application%"
set "watch-cmd=start dotnet watch msbuild %targets% %props%"

cd %modules%\Orchard.Demo & echo off & %watch-cmd%
cd %modules%\Orchard.Body & echo off & %watch-cmd%
cd %application%