@echo off
SET skipdnvm=%1

@IF "%skipdnvm%" == "" SET skipdnvm="0"

@IF NOT "%skipdnvm%" == "1" (
    call dnvm upgrade  -r coreclr -arch x64
)

call dnu restore
