@echo off
del PresalesMonitor.Console.7z >nul 2>&1
echo 	Create package...
"C:\Program Files\7-Zip\7z.exe" a PresalesMonitor.Console.7z ./publish/linux/framework-dependent/* >nul 2>&1 && echo 	Package created successfully. || echo Error. Unable create package. && exit 1
echo 	Send package...
scp -i ~\.ssh\presales_monitor_prod %cd%\PresalesMonitor.Console.7z presale@127.0.0.1:~/  >nul 2>&1 && echo 	Package sent successfully. || echo Error. Unable send package. && exit 1
ssh -i ~\.ssh\presales_monitor_prod presale@127.0.0.1 "./console_deploy" >nul 2>&1 && echo 	Service updated successfully. || echo Error. Something went wrong. NEED TO CONNECT AND FIX. && exit 1