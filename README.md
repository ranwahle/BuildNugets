# BuildNugets
Command line tool to build nuget packages

Usage: BuildNugets -Outputpath "path/to/nugetpackages" -nugetPath "path/to/nugetExecutables" -version versionforallnugets -solutionDir "path/to.solutionDir" -exclude "path/to/excludeddirecrtory"

This code, after build, goes recursively from the solution dir and builds nuget packages whenever it finds nuget spec file (.nuspec)

