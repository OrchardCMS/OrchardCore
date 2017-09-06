# This Docker file is intended for the CI
# A prerequisite is a published application in the .build/release  

FROM microsoft/dotnet:2.0-runtime 
WORKDIR /app

COPY .build/release .

ENV ASPNETCORE_URLS http://+:80
ENTRYPOINT ["dotnet", "OrchardCore.Cms.Web.dll"]
EXPOSE 80
