# This Docker file is intended for the CI
# A prerequisite is a published application in the .build/release  

FROM microsoft/dotnet:2.1-aspnetcore-runtime

EXPOSE 80
ENV ASPNETCORE_URLS http://+:80

WORKDIR /app
COPY .build/release .

ENTRYPOINT ["dotnet", "OrchardCore.Cms.Web.dll"]