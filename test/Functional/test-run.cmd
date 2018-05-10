rd /S /Q ".\publish\App_Data"

start "TESTORCHARD" /D .\publish /B dotnet "OrchardCore.Cms.Web.dll

CALL npm "test"

taskkill /FI "WINDOWTITLE eq TESTORCHARD" /F
