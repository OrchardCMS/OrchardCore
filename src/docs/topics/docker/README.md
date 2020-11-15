# Using Docker with Orchard Core

Our source code repository includes already a `Dockerfile` which will allow you to create your own Docker images and containers. It can be quite usefull for example for Orchard Core developpers when needing to test PR's. It allows them to deploy locally quickly some testing environments.

## Docker

```cmd
REM Folder where the Dockerfile stands
cd /orchardcore/ 

REM Build image from Dockerfile
docker build -t oc .
docker run -p 80:80 oc
```

## Prune intermediary containers

### When using `docker` command : 

```cmd
docker build -t oc --rm .
docker image prune -f --filter label=stage=build-env
docker run -p 80:80 oc
```

## Docker compose

Docker Compose will allow you to deploy everything by doing simply `docker-compose up` command in the root folder of Orchard Core source code. Of course it requires that you have a docker-compose.yml file standing in that folder first.

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
## Prune intermediary containers

### When using `docker-compose` command : 

```cmd
REM Build images if they are not already
docker-compose build

REM Prune intermediary remaining images
docker image prune -f --filter label=stage=build-env

REM Start all containers
docker-compose up
```



