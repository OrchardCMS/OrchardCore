# This Docker file is intended for the CI
# A prerequisite is a published application in the .build/release  

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1

EXPOSE 80
ENV ASPNETCORE_URLS http://+:80

WORKDIR /app
COPY .build/release .

ENTRYPOINT ["dotnet", "OrchardCore.Cms.Web.dll"]