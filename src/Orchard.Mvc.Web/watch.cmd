@echo off
set "application=%cd%"
set "modules=%application%\..\OrchardCore.Modules"
set "themes=%application%\..\OrchardCore.Themes"
set "targets=/t:CopyPackageRazorFiles"
set "props=/p:ApplicationDirectory=%application%"
set "watch=start dotnet watch msbuild %targets% %props%"

cd %modules%\Orchard.Mvc.HelloWorld & %watch%
cd %application%