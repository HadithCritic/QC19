#!/usr/bin/env python3
"""Build the original QuranCode and run OracleDump against it.

The original targets .NET Framework 4.0. Instead of installing the developer
pack, this uses Microsoft's reference-assembly package from NuGet
(Microsoft.NETFramework.ReferenceAssemblies.net40) with the Visual Studio
Build Tools' MSBuild, which is reversible and changes nothing system-wide.

Usage:
    python next/tools/oracle.py --check          # regenerate into a scratch folder, compare with tests/golden
    python next/tools/oracle.py --out <folder>   # regenerate into <folder>
    python next/tools/oracle.py --out <folder> -- <extra OracleDump args>

The build output, C#/Build/Release, is the install root OracleDump runs in.
"""

from __future__ import annotations

import argparse
import io
import os
import shutil
import subprocess
import sys
import tempfile
import urllib.request
import zipfile

HERE = os.path.dirname(os.path.abspath(__file__))
NEXT = os.path.normpath(os.path.join(HERE, ".."))
REPO = os.path.normpath(os.path.join(NEXT, ".."))
LEGACY = os.path.join(REPO, "C#")
INSTALL_ROOT = os.path.join(LEGACY, "Build", "Release")
GOLDEN = os.path.join(NEXT, "tests", "golden")
ORACLE_PROJECT = os.path.join(HERE, "OracleDump", "OracleDump.csproj")
ORACLE_EXE = os.path.join(HERE, "OracleDump", "bin", "Release", "OracleDump.exe")

REFERENCE_PACKAGE = (
    "https://www.nuget.org/api/v2/package/Microsoft.NETFramework.ReferenceAssemblies.net40/1.0.3"
)
REFERENCE_CACHE = os.path.join(LEGACY, "Build", "refasm-net40")
REFERENCE_DIR = os.path.join(REFERENCE_CACHE, "build", ".NETFramework", "v4.0")

# Timing figures change run to run, so they are never compared.
UNCOMPARED = {"performance-baseline.tsv"}


def fail(message: str) -> None:
    print(f"oracle: {message}", file=sys.stderr)
    sys.exit(1)


def find_msbuild() -> str:
    vswhere = os.path.join(os.environ.get("ProgramFiles(x86)", r"C:\Program Files (x86)"),
                           "Microsoft Visual Studio", "Installer", "vswhere.exe")
    if os.path.exists(vswhere):
        found = subprocess.run(
            [vswhere, "-latest", "-products", "*", "-requires", "Microsoft.Component.MSBuild",
             "-find", r"MSBuild\**\Bin\MSBuild.exe"],
            capture_output=True, text=True, check=False).stdout.strip().splitlines()
        if found:
            return found[0]
    fail("MSBuild was not found. Install the Visual Studio Build Tools.")
    return ""


def ensure_reference_assemblies() -> str:
    if os.path.isdir(REFERENCE_DIR):
        return REFERENCE_DIR
    print("oracle: fetching .NET Framework 4.0 reference assemblies from NuGet")
    with urllib.request.urlopen(REFERENCE_PACKAGE, timeout=300) as response:
        package = response.read()
    with zipfile.ZipFile(io.BytesIO(package)) as archive:
        archive.extractall(REFERENCE_CACHE)
    return REFERENCE_DIR


def build(msbuild: str, references: str) -> None:
    for project in (os.path.join(LEGACY, "Solution.sln"), ORACLE_PROJECT):
        print(f"oracle: building {os.path.relpath(project, REPO)}")
        result = subprocess.run(
            [msbuild, project, "-p:Configuration=Release", f"-p:FrameworkPathOverride={references}",
             "-m", "-v:m", "-nologo"],
            capture_output=True, text=True, check=False)
        if result.returncode != 0:
            print(result.stdout[-4000:], file=sys.stderr)
            fail(f"build failed: {project}")


def run(out_dir: str, extra: list[str]) -> None:
    os.makedirs(out_dir, exist_ok=True)
    result = subprocess.run([ORACLE_EXE, INSTALL_ROOT, out_dir, *extra], cwd=INSTALL_ROOT, check=False)
    if result.returncode != 0:
        fail("OracleDump failed")


def data_lines(path: str) -> list[str]:
    with io.open(path, encoding="utf-8-sig") as handle:
        return [line.rstrip("\r\n") for line in handle if not line.startswith("#")]


def compare(out_dir: str) -> bool:
    ok = True
    for name in sorted(os.listdir(GOLDEN)):
        if not name.endswith(".tsv") or name in UNCOMPARED:
            continue
        fresh = os.path.join(out_dir, name)
        if not os.path.exists(fresh):
            print(f"  MISSING  {name}")
            ok = False
        elif data_lines(fresh) != data_lines(os.path.join(GOLDEN, name)):
            print(f"  DIFFERS  {name}")
            ok = False
        else:
            print(f"  same     {name}")
    return ok


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--check", action="store_true", help="regenerate and compare with tests/golden")
    parser.add_argument("--out", help="folder to write the oracle's files into")
    parser.add_argument("--no-build", action="store_true", help="reuse the existing builds")
    parser.add_argument("extra", nargs="*", help="arguments passed to OracleDump after the folders")
    args = parser.parse_args()

    if not args.check and not args.out:
        parser.error("give --check or --out")

    if not args.no_build:
        build(find_msbuild(), ensure_reference_assemblies())

    if args.check:
        scratch = tempfile.mkdtemp(prefix="oracle-")
        try:
            run(scratch, args.extra)
            same = compare(scratch)
        finally:
            shutil.rmtree(scratch, ignore_errors=True)
        print("oracle: golden data reproduces" if same else "oracle: golden data does NOT reproduce")
        return 0 if same else 1

    run(os.path.abspath(args.out), args.extra)
    return 0


if __name__ == "__main__":
    sys.exit(main())
