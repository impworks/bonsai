@echo off

cd ..
dotnet ef database drop -f
dotnet ef database update