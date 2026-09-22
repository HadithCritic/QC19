@echo off
:START

del /F /S /Q *.tmp
del /F /S /Q *.bak
del /F /S /Q *.ini
del /F /S /Q *.user
del /F /S /Q *.pdb
del /F /S /Q *.resources
del /F /S /Q *.exe.config

#rd /S /Q Bookmarks
rd /S /Q History
rd /S /Q Drawings
rd /S /Q Statistics
rd /S /Q Research
rd /S /Q Primes
rd /S /Q Composites

:END
