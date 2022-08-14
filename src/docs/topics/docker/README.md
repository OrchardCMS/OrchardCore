# Using Docker with Orchard Core

Our source code repository includes a `Dockerfile` which will allow you to create your own Docker images and containers. It can be quite useful for Orchard Core developers when needing to test PR's. It allows them to deploy locally quickly some testing environments. Here my examples will be shown for that context. Docker can also be used for more complex usage (ex: production deployment) but this documentation doesn't aim to explain that in detail. For more advanced examples I strongly suggest reading `docker` and `docker-compose` documentation.

*Alternately, for those using a Nuget package solution ; you can copy directly the Dockerfile and .dockerignore file from the source code to the root folder of your solution to do the same kind of thing. Though, it might get tricky depending if you did not use the same folder structure than the source code solution.*

## What you will need

For Windows users : 
https://docs.microsoft.com/en-us/windows/wsl/tutorials/wsl-containers

For Ubuntu/Linux users : https://docs.docker.com/engine/install/ubuntu/

## What you will build

You will build Docker images and containers from command shell using `docker` and `docker-compose` commands. Images are built from Orchard Core source code targeting a specific OS. Then we can deploy "containers" from them. This allow you to see how Orchard Core respond in different environments or also deploy Orchard Core on a production server eventually.

## Dockerfile

The Dockerfile that is provided in the Orchard Core source code is using an intermediate image to build Orchard Core in a specific environment which contains the .NET SDK. Then we create the "real" image by using only the ASP.NET core runtime.

```dockerfile
# Create an intermediate image using .NET Core SDK
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
LABEL stage=build-env
WORKDIR /app

# Copy and build in the intermediate image
COPY ./src /app
RUN dotnet publish /app/OrchardCore.Cms.Web -c Release -o ./build/release

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
EXPOSE 80
ENV ASPNETCORE_URLS http://+:80
WORKDIR /app
COPY --from=build-env /app/build/release .
ENTRYPOINT ["dotnet", "OrchardCore.Cms.Web.dll"]
```

## Dockerignore file ".dockerignore"

```yml
# ignore all
**

# Except src for building
!./src/*

# Ignore any App_Data folder
**/App_Data/

# Ignore all prebuild
**/[b|B]in/
**/[O|o]bj/
```

## Docker

First example is a simple one. Use Docker to build an image and run it (inside a container). 

```cmd
REM Folder where the Dockerfile stands
cd /orchardcore

REM Build image from Dockerfile
docker build -t oc .

REM Creates a container, runs it and expose its service on port 80
docker run -p 80:80 oc
```

## Prune intermediate images

### When using `docker` command : 

```cmd
REM Prunes intermediate containers created while building by using --rm
docker build -t oc --rm .

REM Prunes all intermediate images
docker image prune -f --filter label=stage=build-env

REM Creates a container, runs it and expose its service on port 80
docker run -p 80:80 oc
```

Using these commands should get you a fully functional Docker container running on port 80 so that you can access it with your browser by simply going to http://localhost. Though, we assume that this will only allow you to use SQLite. In order to avoid needing to install anything directly on your Docker host computer and to get everything running quickly you should use `docker-compose`.

## Docker compose

Docker Compose will allow you to generate multiple containers locally by doing simply `docker-compose up` command in the root folder of Orchard Core source code. Of course it requires that you have a docker-compose.yml file standing in that folder first. In the example shown below, we will create services for each of the database providers Orchard Core "officially supports".

[Docker Compose documentation](https://docs.docker.com/compose/)

docker-compose.yml file example :  

```YML
version: '3.3'
services:
    web:
        build: 
            context: .
            dockerfile: Dockerfile
        ports:
            - "5009:80"
        depends_on:
            - sqlserver
            - mysql
            - postgresql
    sqlserver:
        image: "mcr.microsoft.com/mssql/server"
        environment:
            SA_PASSWORD: "P@ssw0rd!123456"
            ACCEPT_EULA: "Y"
    mysql:
        image: mysql:latest
        restart: always
        environment:
            MYSQL_DATABASE: 'orchardcore_database'
            MYSQL_USER: 'orchardcore_user'
            MYSQL_PASSWORD: 'orchardcore_password'
            MYSQL_ROOT_PASSWORD: 'root_password'
        ports:
            - '3306:3306'
        expose:
            - '3306'
        volumes:
            - mysql-data:/var/lib/mysql
    postgresql:
        image: postgres:latest
        volumes:
            - postgresql-data:/var/lib/postgresql/data
        ports:
            - 5432:5432
        environment:
            POSTGRES_USER: orchardcore_user
            POSTGRES_PASSWORD: orchardcore_password
            POSTGRES_DB: orchardcore_database
volumes:
    mysql-data:
    postgresql-data:

```
## Prune intermediate images

### When using `docker-compose` command : 

```cmd
REM Builds images if they are not already built
docker-compose build

REM Prune intermediate remaining images
docker image prune -f --filter label=stage=build-env

REM Start all containers
docker-compose up
```

We added some commands examples to prune intermediate images because our `Dockerfile` uses an intermediate image to do a `dotnet publish` of our source code. If you don't prune your intermediate images ; over time it can certainly take some significant amount of disk space. We label those intermediate images with `stage=build-env` so that they can be pruned easily.

## Create tenants automatically (Autosetup feature)

TODO

See :

[Database shell configurations](../../reference/core/Shells/README.md#database-shells-configuration-provider)

[Autosetup feature PR](https://github.com/OrchardCMS/OrchardCore/pull/4567)

## How can I run my Orchard Core Docker containers over HTTPS?

[Hosting ASP.NET Core Images with Docker over HTTPS](https://github.com/dotnet/dotnet-docker/blob/master/samples/host-aspnetcore-https.md)

## How can I target my Docker images against a specific environment?

Here you can find a list of different environments with a Dockerfile example for each of them.

[.NET Core SDK](https://hub.docker.com/_/microsoft-dotnet-sdk/)  
[ASP.NET Core Runtime](https://hub.docker.com/_/microsoft-dotnet-aspnet/)  

## Why does Orchard Core source code uses a different Dockerfile?

### Dockerfile-CI, Dockerfile-CI.gitignore

Github Actions is the Continuous Integration tool we use to build and test the different branches we have in our repository. It can allow us to create Docker images and containers but building Orchard Core on them would be slower than on the actual CI. So, for that matter, we don't use an intermediate image for building on the CI. Though, it perfectly makes sense to do this locally for yourself as performance should not be limited at all.

## Can I use different Dockerfiles for myself?

You need to use at least Docker version 19.03 to be able to use Docker Buildkit so that it can parse your different .dockerignore files.

See : 

[Build images with BuildKit](https://docs.docker.com/develop/develop-images/build_enhancements/)  
[What is Docker BuildKit and What can I use it for?](https://brianchristner.io/what-is-docker-buildkit/#:~:text=Docker%20BuildKit%20is%20a%20little,and%20increase%20productivity%20for%20free.)  

Github Actions currently supports Buildkit under Linux only.  

See : 

https://github.com/docker/setup-buildx-action#limitation  
https://github.com/OrchardCMS/OrchardCore/issues/7651  
