@echo off
SET skipdnvm=%1

@IF "%skipdnvm%" == "" SET skipdnvm="0"

@IF NOT "%skipdnvm%" == "1" (
    call dnvm upgrade -u -r coreclr -arch x86
    call dnvm upgrade -u -r clr -arch x86
    call dnvm upgrade -u -r coreclr -arch x64
    call dnvm upgrade -u -r clr -arch x64
)

call dnu restore
