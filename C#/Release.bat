@echo off
REM ---------------------------------------------------------------------------
REM Superseded by build.ps1.
REM
REM The original script moved all 30 source directories into a temporary "C#"
REM folder, zipped it, moved them back, and finished with "RD /S /Q C#". No step
REM checked whether it had succeeded, so if any file was locked and failed to
REM move back, the recursive delete destroyed it. It also invoked 7-Zip from a
REM hardcoded path; where 7-Zip is not installed, the archive steps failed
REM silently and the script deleted the folder anyway.
REM
REM build.ps1 copies into a staging directory under TEMP instead of moving the
REM source tree, checks every step, and compresses with .NET rather than an
REM external tool. The original is recoverable from git history.
REM ---------------------------------------------------------------------------

powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0build.ps1" -Package %*
exit /b %ERRORLEVEL%
