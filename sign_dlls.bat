@ECHO OFF

SET dllFolder=%1
SET certFile=%2
SET certPass=%3

for /r "%dllFolder%" %%F in (*.dll) do signtool sign /f "%certFile%" /p "%certPass%" /v /tr http://timestamp.digicert.com?alg=sha256 /td SHA256 /fd SHA256 "%%F"
goto :eof

:usage
@echo Usage: %0 "<PATH_TO_DLL_FOLDER>" "<PATH_TO_CERTIFICATE>" "<CERTIFICATE_PASSWORD>"
exit /B 1