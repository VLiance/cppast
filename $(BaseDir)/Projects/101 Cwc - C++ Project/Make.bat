@echo off
::   /------- Find Cwc (if not set in your Environement variables) ---------/
set cwc=cwc
FOR /L %%G IN (1,1,5) DO (if not exist cwc.exe ( cd.. ))

::  /--  Run Cwc.exe, with this batch file folder as working directory (% -wDir %~dp0) and pass all is arguments (%*)    --/
::  /--  All arguments lines finish with a pipe ("|"^) which mean a parallel actions, or (">"^) for sequentials actions  --/
 
::  /-------------------Start----------------------/
 %cwc% -wDir %~dp0 %* "|"^
 -I. -c src/ -o obj/(wPlatform)/ -wTo out/(wPlatform)/App.exe -O2 -std=c++11 "|"^
 
::  /--------------------End----------------------/