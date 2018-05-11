CALL npm install jest jest-cli puppeteer --save-dev
dotnet publish -c Release -o %cd%/publish ../../src/OrchardCore.Cms.Web/OrchardCore.Cms.Web.csproj
