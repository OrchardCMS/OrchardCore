@echo off
SET skipdnvm=%1

@IF "%skipdnvm%" == "" SET skipdnvm="0"

@IF NOT "%skipdnvm%" == "1" (
    call dnvm upgrade  -r coreclr -arch x64
)

call dnu clear-http-cache
cd src/orchard.web/modules
call dnu restore
cd../core
call dnu restore
cd../../..
call dnu restore
