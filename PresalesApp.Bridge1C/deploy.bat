@echo off
cd /d D:\users\polyakov\github\PresalesApp\PresalesApp.Bridge1C
set pkg_name=PresalesApp.Bridge1C
del %pkg_name%.7z >nul 2>&1
echo    Create %pkg_name% package...
"C:\Program Files\7-Zip\7z.exe" a %pkg_name%.7z "./bin/Publish/Linux SC/*" >nul 2>&1 && echo    [102mPackage created successfully.[0m || echo    [101mError. Unable to create package.[0m && exit 1
echo    Send %pkg_name% package...
scp -i ~\.ssh\presales_monitor_prod %cd%\%pkg_name%.7z presale@127.0.0.1:~/ >nul 2>&1 && echo    [102mPackage sent successfully.[0m || echo    [101mError. Unable to send package.[0m && exit 1
echo    Deploy %pkg_name% service...
ssh -i ~\.ssh\presales_monitor_prod presale@127.0.0.1 "./bridge_deploy" >nul 2>&1 && echo    [102mService updated successfully.[0m || echo    [101mError. Something went wrong. NEED TO CONNECT AND FIX.[0m
pause