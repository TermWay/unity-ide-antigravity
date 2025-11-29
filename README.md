# Unity Antigravity Editor

This package provides **native** integration between Unity and the [Antigravity Editor](https://antigravity.google/).

It is a **fork** of the legacy `com.unity.ide.vscode` package, patched specifically to recognize Antigravity as a supported IDE. This solves the issue where Unity treats the editor as a generic text tool by:
- Automatically detecting `Antigravity.exe` and `.app` paths.
- Forcing the generation of `.csproj` and `.sln` files.
- Enabling full IntelliSense support via OmniSharp.

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
5. Paste the following URL and click **Add**: 
   `https://github.com/TermWay/unity-ide-antigravity.git`

## Setup
After installing:
1. Go to **Edit** > **Preferences** > **External Tools**.
2. Select **Antigravity Editor** from the dropdown.
3. Click **Regenerate project files**.

<img src="https://raw.githubusercontent.com/TermWay/unity-ide-antigravity/main/Documentation~/Images/antigravity-setup.png" width="600" alt="Antigravity Setup">
<img src="https://raw.githubusercontent.com/TermWay/unity-ide-antigravity/main/Documentation~/Images/antigravity-preview.png" width="600" alt="Antigravity Code Preview">