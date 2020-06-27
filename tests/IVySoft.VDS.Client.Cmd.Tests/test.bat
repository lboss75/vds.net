set src_path=..\..\..\vds\out\build\x86-Debug

xcopy /y %src_path%\app\vds_background\vds_background.* bin\Debug\netcoreapp3.1\
xcopy /y %src_path%\app\vds_ws_server\vds_ws_server.* bin\Debug\netcoreapp3.1\
xcopy /y ..\..\..\vds\keys bin\Debug\netcoreapp3.1\
rem dotnet test