
$orchardwebDirectory = [System.IO.Path]::GetFullPath(".\src\Orchard.Web")

invoke-expression ($orchardwebDirectory + "\dnx web")
