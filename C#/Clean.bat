@echo off
:START

cd Tools
CALL Clean
cd ..

del /F    /Q *.zip
del /F /S /Q *.tmp
del /F /S /Q *.bak
del /F /S /Q *.ini
del /F /S /Q *.user
del /F /S /Q *.pdb
del /F /S /Q *.resources
del /F /S /Q *.vshost.exe
del /F /S /Q *.vshost.exe.manifest
del /F /S /Q *.exe.config

rd /S /Q Build\Debug

rd /S /Q Build\Release\Bookmarks
rd /S /Q Build\Release\History
rd /S /Q Build\Release\Drawings
rd /S /Q Build\Release\Statistics
rd /S /Q Build\Release\Research
rd /S /Q Build\Release\Primes
rd /S /Q Build\Release\Composites
rd /S /Q Build\Release\Deficients

rd /S /Q Maths\obj
rd /S /Q Globals\obj
rd /S /Q Utilities\obj
rd /S /Q Model\obj
rd /S /Q DataAccess\obj
rd /S /Q Server\obj
rd /S /Q Client\obj
rd /S /Q Research\obj
rd /S /Q QuranCode\obj
rd /S /Q ScriptRunner\obj
rd /S /Q AhlulBayt\obj
rd /S /Q PrimeCalculator\obj
rd /S /Q QuranLab\obj
rd /S /Q QuranNet\obj
rd /S /Q InitialLetters\obj
rd /S /Q Primes\obj
rd /S /Q Composites\obj
rd /S /Q Deficients\obj
rd /S /Q Dimensions\obj
rd /S /Q Divisibility\obj
rd /S /Q DayOfWeek\obj
rd /S /Q Indices\obj
rd /S /Q Numbers\obj
rd /S /Q WordDecoder\obj
rd /S /Q WordFinder\obj
rd /S /Q WordGenerator\obj

:END
