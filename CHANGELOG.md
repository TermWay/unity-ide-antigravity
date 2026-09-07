# Code Editor Package for Visual Studio Code

## [1.1.0] - 2026-09-07

### Fixed
- **Antigravity 2.0 split (Google I/O, 2026-05-19):** Google split Antigravity into two desktop apps that share the same VS Code-fork shell: the standalone **Antigravity** agent app (no code editor) and **Antigravity IDE** (the editor). The package used to match the bare `Antigravity` executable, so Unity ended up launching the agent app on every script double-click. Only Antigravity IDE is now discovered and accepted.

### Changed
- **Discovery:** Looks for `Antigravity IDE.exe` (Windows), `Antigravity IDE*.app` (macOS) and `antigravity-ide[-insiders]` (Linux) in their default install locations. The bare `Antigravity` locations are no longer offered as script editors.
- **Validation:** A manually selected path (Browse...) must have an Antigravity IDE file name, and its `resources/app/package.json` manifest `name` is checked so the agent app is rejected even when pointed at directly.
- **Migration:** When the saved External Script Editor still points at the bare `Antigravity` app and Antigravity IDE is installed, the selection is switched to Antigravity IDE automatically (a message is logged in the Console). When Antigravity IDE is not installed, a warning is logged instead.
- The External Tools dropdown entry is now named **Antigravity IDE** (and **Antigravity IDE - Insider** for prerelease builds) instead of the ambiguous **Antigravity**.

## [1.0.0] - 2025-11-29

### Added
- **Antigravity Support:** Added logic to discover and register `Antigravity.exe` (Windows) and `Antigravity.app` (macOS) as a supported external script editor.
- **Namespace Refactor:** Renamed all namespaces from `VSCodeEditor` to `Antigravity.Editor` to prevent conflicts with the legacy `com.unity.ide.vscode` package.
- **Project Generation:** Enabled .csproj and .sln generation specifically for Antigravity, bypassing the need for Microsoft's proprietary "C# Dev Kit".

### Changed
- Forked from `com.unity.ide.vscode` version 1.2.5.
- Changed package name to `com.termway.ide.antigravity`.
- Changed display name to "Antigravity Editor".
- Updated `package.json` dependencies (removed test framework dependencies).

### Fixed
- Fixed a case-sensitivity bug in `TryGetInstallationForPath` that caused `Antigravity.exe` to be rejected during validation.

## Legacy History (com.unity.ide.vscode)

## [1.2.5] - 2022-02-07

- Introduce OnGeneratedCSProjectFiles, OnGeneratedCSProject and OnGeneratedSlnSolution callbacks.
- Always use forward slash in source paths
- Analyzers use absolute paths
- Ruleset files for roslyn analyzers
- Extra snap search paths on Ubuntu
- Specific c# language version for specific unity versions
- No longer hide .gitignore in VSCode file explorer


## [1.2.3] - 2020-10-23

Remove workaround for VSCode omnisharp (as of https://github.com/OmniSharp/omnisharp-vscode/issues/4113 we no longer need to disable the referenceoutputassemblies).


## [1.2.2] - 2020-09-04

VSC-14 - synchronize solution file when adding new assembly


## [1.2.1] - 2020-05-15

Source filtering adds support for asmref


## [1.2.0] - 2020-03-04

Do not reference projects that has not been generated (case 1211057)
Only open files that exists (case 1188394)
Add individual toggle buttons for generating csprojects for packages
Add support for Roslyn analyzers in project generation through csc.rsp and compiled assembly references
Remove Release build target from csproj and sln


## [1.1.4] - 2020-01-02

Delta project generation, only recompute the csproj files whose script modified.


## [1.1.3] - 2019-10-22

Exe version of vscode will use Normal ProcessWindowStyle while cmd will use Hidden


## [1.1.2] - 2019-08-30

Fixing OSX open command arguments


## [1.1.1] - 2019-08-19

Support for Player Project. Generates specific csproj files containing files, reference, defines,
etc. that will show how the assembly will be compiled for a target platform.


## [1.1.0] - 2019-08-07

Adds support for choosing extensions to be opened with VSCode. This can be done through the GUI in Preferences.
Avoids opening all extensions after the change in core unity.


## [1.0.7] - 2019-05-15

Fix various OSX specific issues.
Generate project on load if they are not generated.
Fix path recognition.


## [1.0.6] - 2019-04-30

Ensure asset database is refreshed when generating csproj and solution files.

## [1.0.5] - 2019-04-27

Add support for generating all csproj files.

## [1.0.4] - 2019-04-18

Fix relative package paths.
Fix opening editor on mac.
Add %LOCALAPPDATA%/Programs to the path of install paths.

## [1.0.3] - 2019-01-01

### This is the first release of *Unity Package vscode_editor*.

Using the newly created api to integrate Visual Studio Code with Unity.
