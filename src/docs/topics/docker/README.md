# Using Docker with Orchard Core

Our source code repository includes a `Dockerfile` which will allow you to create your own Docker images and containers. It can be quite usefull for example for Orchard Core developpers when needing to test PR's. It allows them to deploy locally quickly some testing environments. Here my examples will be shown for that context. Docker can also be used for more complex usage (ex: production deployment) but this documentation doesn't aim to explain that in detail. For more advanced examples I strongly suggest reading `docker` and `docker-compose` documentation.

## Docker

```cmd
REM Folder where the Dockerfile stands
cd /orchardcore/ 

REM Build image from Dockerfile
docker build -t oc .
docker run -p 80:80 oc
```

## Prune intermediary images

### When using `docker` command : 

```cmd
docker build -t oc --rm .
docker image prune -f --filter label=stage=build-env
docker run -p 80:80 oc
```

Using these commands should get you a fully functional docker container running on port 80 so that you can access it with your browser by simply going to http://localhost. Though we assume that this will only allow you to use SQLite. For avoiding needing to install anything directly on your Docker host computer you will need to use `docker-compose`.

## Docker compose

Docker Compose will allow you to deploy everything by doing simply `docker-compose up` command in the root folder of Orchard Core source code. Of course it requires that you have a docker-compose.yml file standing in that folder first. In the example shown below, we will create services for each of the database providers Orchard Core "supports officially".

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
## Prune intermediary images

### When using `docker-compose` command : 

```cmd
REM Build images if they are not already
docker-compose build

REM Prune intermediary remaining images
docker image prune -f --filter label=stage=build-env

REM Start all containers
docker-compose up
```

I've added some commands examples to prune intermediary images because our `Dockerfile` uses an intermediary image to do a `dotnet publish` of our source code. If you don't prune your intermediary images ; over time it can certainly take some disk space. We label those intermediary images with `stage=build-env` so that they can be removed easily.

## Create tenants automatically (Autosetup feature)

TODO

See PR https://github.com/OrchardCMS/OrchardCore/pull/4567