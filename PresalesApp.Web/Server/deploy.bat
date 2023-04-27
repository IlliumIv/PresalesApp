@echo off
set pkg_name=PresalesApp.Web.Server
del %pkg_name%.7z >nul 2>&1
echo    Create package...
"C:\Program Files\7-Zip\7z.exe" a %pkg_name%.7z "./bin/Publish/Linux SC/*" >nul 2>&1 && echo    [102mPackage created successfully.[0m || echo    [101mError. Unable to create package.[0m && exit 1
echo    Send package...
scp -i ~\.ssh\presales_monitor_prod %cd%\%pkg_name%.7z presale@127.0.0.1:~/  >nul 2>&1 && echo    [102mPackage sent successfully.[0m || echo    [101mError. Unable to send package.[0m && exit 1