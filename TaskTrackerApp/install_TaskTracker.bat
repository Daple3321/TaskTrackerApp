@echo off
title Task Tracker install
echo installing TaskTrackerApp wait... 
dotnet tool install --global --add-source ./nupkg TaskTrackerApp
pause