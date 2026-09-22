@echo off
IF EXIST Release.bat GOTO :END

XCOPY /Q /H /E /Y C#\DataAccess\Audio\*.*        .\Audio\
XCOPY /Q /H /E /Y C#\DataAccess\Data\*.*         .\Data\
XCOPY /Q /H /E /Y C#\DataAccess\Translations\*.* .\Translations\
XCOPY /Q /H /E /Y C#\InitialLetters\Data\*.*     .\Data\
XCOPY /Q /H /E /Y C#\Model\Data\*.*              .\Data\
XCOPY /Q /H /E /Y C#\PrimeCalculator\Tools\*.*   .\Tools\
XCOPY /Q /H /E /Y C#\QuranNet\3dQuran\*.*        .\3dQuran\
XCOPY /Q /H /E /Y C#\QuranCode\Fonts\*.*         .\Fonts\
XCOPY /Q /H /E /Y C#\QuranCode\Help\*.*          .\Help\
XCOPY /Q /H /E /Y C#\QuranCode\Icons\*.*         .\Icons\
XCOPY /Q /H /E /Y C#\QuranCode\Images\*.*        .\Images\
XCOPY /Q /H /E /Y C#\QuranCode\Languages\*.*     .\Languages\
XCOPY /Q /H /E /Y C#\QuranCode\UserText\*.*      .\UserText\
XCOPY /Q /H /E /Y C#\ScriptRunner\Scripts\*.*    .\Scripts\
XCOPY /Q /H /E /Y C#\Server\Rules\*.*            .\Rules\
XCOPY /Q /H /E /Y C#\Server\Values\*.*           .\Values\
XCOPY /Q /H /E /Y C#\Utilities\Numbers\*.*       .\Numbers\
XCOPY /Q /H /E /Y C#\Divisibility\Help\*.*       .\Help\
XCOPY /Q /H /E /Y C#\DayOfWeek\Help\*.*          .\Help\
XCOPY /Q /H /E /Y C#\WordDecoder\Data\*.*        .\Data\
COPY /Y C#\LICENSE .
COPY /Y Build\Release\*.* .
RD /S /Q Build
DEL Setup.bat

:END
