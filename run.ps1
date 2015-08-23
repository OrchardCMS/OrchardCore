
$orchardwebDirectory = [System.IO.Path]::GetFullPath(".\src\OrchardVNext.Web")

invoke-expression ($orchardwebDirectory + "\dnx web")
