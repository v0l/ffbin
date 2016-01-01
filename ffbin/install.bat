@echo off

xcopy bin\Release\ffbin.exe %WINDIR% /Y
xcopy bin\Release\HtmlAgilityPack.dll %WINDIR% /Y
xcopy bin\Release\Newtonsoft.Json.dll %WINDIR% /Y

sc create ffbin BinPath=%WINDIR%\ffbin.exe start=auto DisplayName=fbin

pause