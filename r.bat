@echo off
dotnet build
if %errorlevel% neq 0 exit /b %errorlevel%
dotnet run