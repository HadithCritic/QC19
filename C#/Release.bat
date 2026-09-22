@echo off

CALL Clean.bat
CALL Version.bat

REM -- Move source code to C# folder:
IF NOT EXIST C# MD C#
MOVE /Y LICENSE C#
MOVE /Y Readme.txt C#
MOVE /Y Clean.bat C#
MOVE /Y *.txt C#
MOVE /Y *.sln C#
MOVE /Y Tools C#
MOVE /Y Maths C#
MOVE /Y Globals C#
MOVE /Y Utilities C#
MOVE /Y Model C#
MOVE /Y DataAccess C#
MOVE /Y Server C#
MOVE /Y Client C#
MOVE /Y Research C#
MOVE /Y QuranCode C#
MOVE /Y ScriptRunner C#
MOVE /Y AhlulBayt C#
MOVE /Y PrimeCalculator C#
MOVE /Y QuranLab C#
MOVE /Y QuranNet C#
MOVE /Y InitialLetters C#
MOVE /Y Primes C#
MOVE /Y Composites C#
MOVE /Y Deficients C#
MOVE /Y Dimensions C#
MOVE /Y Divisibility C#
MOVE /Y DayOfWeek C#
MOVE /Y Indices C#
MOVE /Y Numbers C#
MOVE /Y WordDecoder C#
MOVE /Y WordFinder C#
MOVE /Y WordGenerator C#
COPY /Y Version.bat C#
COPY /Y Release.bat C#
COPY /Y Setup.bat C#
XCOPY /H *.suo C#

REM -- Archive the C# folder:
"%PROGRAMFILES%\7-Zip\7z.exe" a -tzip -mx5 QuranCode1433.zip C#
"%PROGRAMFILES%\7-Zip\7z.exe" a -tzip -mx5 QuranCode1433.zip Setup.bat

REM -- Move back the contents of C# folder to their original location:
MOVE /Y C#\LICENSE .
MOVE /Y C#\README.md .
MOVE /Y C#\Clean.bat .
MOVE /Y C#\Setup.bat .
MOVE /Y C#\*.txt .
MOVE /Y C#\*.sln .
MOVE /Y C#\Maths .
MOVE /Y C#\Tools .
MOVE /Y C#\Globals .
MOVE /Y C#\Utilities .
MOVE /Y C#\Model .
MOVE /Y C#\DataAccess .
MOVE /Y C#\Server .
MOVE /Y C#\Client .
MOVE /Y C#\Research .
MOVE /Y C#\QuranCode .
MOVE /Y C#\ScriptRunner .
MOVE /Y C#\AhlulBayt .
MOVE /Y C#\PrimeCalculator .
MOVE /Y C#\QuranLab .
MOVE /Y C#\QuranNet .
MOVE /Y C#\InitialLetters .
MOVE /Y C#\Primes .
MOVE /Y C#\Composites .
MOVE /Y C#\Deficients .
MOVE /Y C#\Dimensions .
MOVE /Y C#\Divisibility .
MOVE /Y C#\DayOfWeek .
MOVE /Y C#\Indices .
MOVE /Y C#\Numbers .
MOVE /Y C#\WordDecoder .
MOVE /Y C#\WordFinder .
MOVE /Y C#\WordGenerator .

REM // delete C# folder.
RD /S /Q C#

REM // add the contents of the Build\Release folder to the archive.
"%PROGRAMFILES%\7-Zip\7z.exe" a -tzip  QuranCode1433.zip Build\Release\*.bat
"%PROGRAMFILES%\7-Zip\7z.exe" a -tzip  QuranCode1433.zip Build\Release\*.txt
"%PROGRAMFILES%\7-Zip\7z.exe" a -tzip  QuranCode1433.zip Build\Release\*.dll
"%PROGRAMFILES%\7-Zip\7z.exe" a -tzip  QuranCode1433.zip Build\Release\*.exe

CALL Version.bat
