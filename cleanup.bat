@echo off
echo Deleting all bin and obj folders...
for /d /r %%d in (bin,obj) do (
    if exist "%%d" (
        echo Deleting: %%d
        rmdir /s /q "%%d"
    )
)
echo Done.
