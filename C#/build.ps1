<#
.SYNOPSIS
    Builds QuranCode and optionally packages a release archive.

.DESCRIPTION
    Replaces the original Release.bat, which moved all 30 source directories
    into a temporary "C#" folder, zipped it, moved them back, and then ran
    "RD /S /Q C#". None of those steps checked whether they succeeded, so a
    single locked file left source inside that folder when the recursive
    delete ran.

    This script never moves or deletes the source tree. It copies what it
    needs into a staging directory under TEMP and removes only that.

.PARAMETER Configuration
    Release (default) or Debug.

.PARAMETER Package
    Also produce the distributable .zip archive.

.PARAMETER Stamp
    Run the version-stamping tools before building. These rewrite tracked
    source (Globals/Globals.cs and the AssemblyInfo files), so it is off by
    default. Review the result with "git diff" before committing.

.PARAMETER Force
    Allow packaging with a dirty working tree.

.EXAMPLE
    .\build.ps1
    .\build.ps1 -Package
#>
[CmdletBinding()]
param(
    [ValidateSet('Release', 'Debug')]
    [string] $Configuration = 'Release',
    [switch] $Package,
    [switch] $Stamp,
    [switch] $Force
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$SourceRoot = $PSScriptRoot
$Solution   = Join-Path $SourceRoot 'Solution.sln'
$OutputDir  = Join-Path $SourceRoot "Build\$Configuration"

function Write-Step { param([string] $Message) Write-Host "==> $Message" -ForegroundColor Cyan }
function Write-Note { param([string] $Message) Write-Host "    $Message" -ForegroundColor DarkGray }

# ---------------------------------------------------------------------------
# Toolchain discovery
# ---------------------------------------------------------------------------
# These are legacy MSBuild 2003 projects targeting .NET Framework 4.0 Client
# Profile. MSBuild 17 refuses them outright (MSB3644) unless a .NET Framework
# Developer Pack is installed, because it will not silently resolve references
# from the GAC. MSBuild 4.0 will, with warnings, so it stays the default until
# the projects are retargeted.
function Find-MSBuild {
    $legacy = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe'
    if (Test-Path $legacy) { return $legacy }

    $modern = Get-ChildItem -Path @(
        "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2022",
        "$env:ProgramFiles\Microsoft Visual Studio\2022"
    ) -Filter 'MSBuild.exe' -Recurse -ErrorAction SilentlyContinue |
        Where-Object { $_.FullName -match '\\Bin\\MSBuild\.exe$' } |
        Select-Object -First 1
    if ($modern) { return $modern.FullName }

    throw 'No MSBuild found. Install the Visual Studio Build Tools.'
}

# The C# 4.0 compiler signs assemblies by importing the key into a machine-level
# CryptoAPI container, which needs write access to
# %ALLUSERSPROFILE%\Microsoft\Crypto\RSA\MachineKeys. On machines where that
# folder has lost its default write ACE, signing Maths.csproj fails with a bare
# "CS1548 ... Access is denied".
#
# Roslyn signs in managed code and never touches that folder, so pointing the
# old MSBuild at the newer compiler fixes signing without an ACL change, an
# elevation prompt, or dropping the strong name.
function Find-Roslyn {
    $candidates = Get-ChildItem -Path @(
        "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2022",
        "$env:ProgramFiles\Microsoft Visual Studio\2022"
    ) -Filter 'csc.exe' -Recurse -ErrorAction SilentlyContinue |
        Where-Object { $_.FullName -match '\\Roslyn\\csc\.exe$' } |
        Select-Object -First 1
    if ($candidates) { return Split-Path $candidates.FullName -Parent }
    return $null
}

# ---------------------------------------------------------------------------
# Optional version stamping
# ---------------------------------------------------------------------------
function Invoke-Stamp {
    Write-Step 'Stamping version (rewrites tracked source)'
    $tools = @{
        Version = Join-Path $SourceRoot 'Tools\Version\bin\Release\Version.exe'
        Touch   = Join-Path $SourceRoot 'Tools\Touch\bin\Release\Touch.exe'
        Replace = Join-Path $SourceRoot 'Tools\Replace\bin\Release\Replace.exe'
    }
    foreach ($kv in $tools.GetEnumerator()) {
        if (-not (Test-Path $kv.Value)) {
            throw "Missing build tool: $($kv.Value). These are tracked binaries; restore them with 'git checkout -- Tools'."
        }
    }

    Push-Location $SourceRoot
    try {
        & $tools.Version '.'     '7.29.139.8317' '7.29.139.8317' '-Tools'
        & $tools.Touch   '.'     '14:33'                         '-Tools'
        & $tools.Touch   'Build' '14:33'                         '-Tools'
        & $tools.Touch   'Tools' '2009-07-29' '07:29'
        & $tools.Replace 'Globals' 'Globals.cs' 'RELEASE' 'B89'
    }
    finally { Pop-Location }
    Write-Note 'Source was modified. Review with: git diff'
}

# ---------------------------------------------------------------------------
# Build
# ---------------------------------------------------------------------------
if ($Stamp) { Invoke-Stamp }

$msbuild = Find-MSBuild
$roslyn  = Find-Roslyn

Write-Step "Building $Configuration"
Write-Note "MSBuild: $msbuild"

$arguments = @(
    $Solution
    "-p:Configuration=$Configuration"
    '-nologo'
    '-verbosity:minimal'
    '-maxcpucount'
)

if ($roslyn) {
    Write-Note "Roslyn:  $roslyn (enables strong-name signing)"
    $arguments += "-p:CscToolPath=$roslyn"
}
else {
    Write-Warning @'
No Roslyn compiler found, so Maths.csproj will be signed by the .NET 4.0
compiler. If that fails with CS1548 "Access is denied", either install the
Visual Studio Build Tools or restore the default write ACE on
%ALLUSERSPROFILE%\Microsoft\Crypto\RSA\MachineKeys.
'@
}

& $msbuild @arguments
if ($LASTEXITCODE -ne 0) { throw "Build failed with exit code $LASTEXITCODE." }

$artifacts = @(Get-ChildItem -Path (Join-Path $OutputDir '*') -Include '*.exe', '*.dll' -File -ErrorAction SilentlyContinue)
if ($artifacts.Count -eq 0) { throw "Build reported success but produced no artifacts in $OutputDir." }
Write-Step "Build succeeded: $($artifacts.Count) artifacts in $OutputDir"

if (-not $Package) { return }

# ---------------------------------------------------------------------------
# Package
# ---------------------------------------------------------------------------
# Version stamping rewrites tracked files, so refuse to package on top of
# changes that have not been reviewed.
if (-not $Force) {
    $git = Get-Command git -ErrorAction SilentlyContinue
    if ($git) {
        $dirty = & git -C $SourceRoot status --porcelain 2>$null
        if ($LASTEXITCODE -eq 0 -and $dirty) {
            throw "Working tree has uncommitted changes. Commit them, or pass -Force.`n$dirty"
        }
    }
}

$release = 'B89'
$globals = Join-Path $SourceRoot 'Globals\Globals.cs'
if (Test-Path $globals) {
    $match = Select-String -Path $globals -Pattern 'RELEASE\s*=\s*"([^"]+)"' | Select-Object -First 1
    if ($match) { $release = $match.Matches[0].Groups[1].Value }
}

$archive = Join-Path (Split-Path $SourceRoot -Parent) "QuranCode1433.$release.zip"

# Staging lives under TEMP and is the ONLY thing this script deletes.
$staging = Join-Path ([IO.Path]::GetTempPath()) ("QuranCodeRelease_" + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $staging -Force | Out-Null

try {
    Write-Step "Staging release into $staging"

    # Source, minus build output and VS user state. Copied, never moved.
    $sourceStage = Join-Path $staging 'C#'
    $excludeDirs = '\\(Build|obj|bin|\.vs|\.git)\\'
    New-Item -ItemType Directory -Path $sourceStage -Force | Out-Null

    Get-ChildItem -Path $SourceRoot -Recurse -File | Where-Object {
        $relative = $_.FullName.Substring($SourceRoot.Length)
        ($relative -notmatch $excludeDirs) -and ($_.Extension -notin '.suo', '.user')
    } | ForEach-Object {
        $relative = $_.FullName.Substring($SourceRoot.Length).TrimStart('\')
        $target   = Join-Path $sourceStage $relative
        $parent   = Split-Path $target -Parent
        if (-not (Test-Path $parent)) { New-Item -ItemType Directory -Path $parent -Force | Out-Null }
        Copy-Item -LiteralPath $_.FullName -Destination $target -Force
    }

    # Setup.bat sits at the archive root; the end user runs it after extracting.
    $setup = Join-Path $SourceRoot 'Setup.bat'
    if (-not (Test-Path $setup)) { throw "Setup.bat not found at $setup." }
    Copy-Item -LiteralPath $setup -Destination (Join-Path $staging 'Setup.bat') -Force

    # Built binaries, under their original Build\Release path.
    $binStage = Join-Path $staging "Build\$Configuration"
    New-Item -ItemType Directory -Path $binStage -Force | Out-Null
    $binaries = @(Get-ChildItem -Path (Join-Path $OutputDir '*') -Include '*.bat', '*.txt', '*.dll', '*.exe' -File)
    if ($binaries.Count -eq 0) { throw "No binaries to package in $OutputDir." }
    $binaries | Copy-Item -Destination $binStage -Force

    Write-Step "Compressing to $archive"
    if (Test-Path $archive) { Remove-Item -LiteralPath $archive -Force }

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [IO.Compression.ZipFile]::CreateFromDirectory(
        $staging, $archive, [IO.Compression.CompressionLevel]::Optimal, $false)

    if (-not (Test-Path $archive)) { throw 'Archive was not created.' }
    $size = [Math]::Round((Get-Item $archive).Length / 1MB, 1)
    Write-Step "Packaged $archive ($size MB)"
}
finally {
    # Guarded: only ever removes the staging directory this script created.
    if ($staging -and $staging.StartsWith([IO.Path]::GetTempPath()) -and (Test-Path $staging)) {
        Remove-Item -LiteralPath $staging -Recurse -Force -ErrorAction SilentlyContinue
    }
}
