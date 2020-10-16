FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
LABEL stage=build-env
WORKDIR /app

# Copy and build
COPY . /app
RUN dotnet publish ./src/OrchardCore.Cms.Web -c Release -o ./build/release

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
EXPOSE 80
ENV ASPNETCORE_URLS http://+:80
WORKDIR /app
COPY --from=build-env /app/build/release .
ENTRYPOINT ["dotnet", "OrchardCore.Cms.Web.dll"]
