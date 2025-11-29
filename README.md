# Unity Antigravity Editor

This package provides proper integration between Unity and the [Antigravity Editor](https://www.antigravity.dev/). 

It solves the issue where Unity treats Antigravity as a generic text editor by:
- Automatically detecting `Antigravity.exe`.
- Forcing the generation of `.csproj` and `.sln` files.
- Enabling full IntelliSense support.

## Important Requirement
Because Antigravity is a VS Code fork, it cannot use the Microsoft "C# Dev Kit". 
To get IntelliSense working, you **must**:
1. Install this package in Unity.
2. Install the **C# (OmniSharp)** extension (by `muhammad-sammy`) inside Antigravity.

## Installation

You can install this package directly via the Unity Package Manager using the Git URL.

### Unity Package Manager (Recommended)
1. Open your Unity project.
2. Go to **Window** > **Package Manager**.
3. Click the **+** (plus) button in the top-left corner.
4. Select **Add package from git URL...**.
5. Paste the following URL and click **Add**: `https://github.com/TermWay/unity-ide-antigravity.git`