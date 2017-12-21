# Release

To release to nuget:

1. Open the solution with Visual Studio.
2. Set the configuration to Release/AnyCPU.
3. Edit project properties under Package to increment the FileVersion.
4. Build.
5. Navigate to the `OhNoPub.ImplicitCastAnalyzer/OhNoPub.ImplicitCastAnalyzer/bin/Release` directory.
6. Run `nuget push -Source OhNoPub.ImplicitCastAnalyzer/ «packageName».nupkg «API key»
