@echo off

cd ..
del /f /q ".\Data\Migrations\*.cs"
dotnet ef database drop -f

dotnet ef migrations add Initial -o "Data\Migrations"
dotnet ef database update