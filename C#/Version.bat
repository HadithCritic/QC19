COPY Readme.txt   Build\Release\
COPY QuranCode\Features.txt Build\Release\

Tools\Version\bin\Release\Version.exe .              7.29.139.8317 7.29.139.8317  -Tools

Tools\Touch\bin\Release\Touch.exe .                 14:33                         -Tools
Tools\Touch\bin\Release\Touch.exe Build             14:33                         -Tools
Tools\Touch\bin\Release\Touch.exe Tools  2009-07-29 07:29

Tools\Replace\bin\Release\Replace.exe Globals Globals.cs RELEASE B89
COPY QuranCode1433.zip                       ..\..\QuranCode1433.B89.zip
DEL  QuranCode1433.zip
