# Unity Antigravity Editor

This package provides **native** integration between Unity and [Antigravity IDE](https://antigravity.google/).

It is a **fork** of the legacy `com.unity.ide.vscode` package, patched specifically to recognize Antigravity IDE as a supported IDE. This solves the issue where Unity treats the editor as a generic text tool by:
- Automatically detecting `Antigravity IDE.exe`, `Antigravity IDE.app` and `antigravity-ide` paths.
- Forcing the generation of `.csproj` and `.sln` files.
- Enabling full IntelliSense support via OmniSharp.

> **Antigravity 2.0 (Google I/O, May 2026):** Antigravity is now two separate apps. The standalone **Antigravity** app is an agent-orchestration tool with no code editor; **Antigravity IDE** is the VS Code fork you write code in. Version 1.1.0 of this package only registers Antigravity IDE. See [Antigravity 2.0 split](#antigravity-20-split) if Unity started opening the agent app instead of your scripts.

## Important Requirement
Because Antigravity is a VS Code fork, it cannot use the Microsoft "C# Dev Kit". 
To get IntelliSense working, you **must**:
1. Install this package in Unity.
2. Install the **[free-csharp-vscode](https://open-vsx.org/extension/muhammad-sammy/csharp)** extension (by `muhammad-sammy`) inside Antigravity.

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
2. Select **Antigravity IDE** from the dropdown.
3. Click **Regenerate project files**.

<img src="https://raw.githubusercontent.com/TermWay/unity-ide-antigravity/main/Documentation~/Images/antigravity-setup.png" width="600" alt="Antigravity Setup">
<img src="https://raw.githubusercontent.com/TermWay/unity-ide-antigravity/main/Documentation~/Images/antigravity-preview.png" width="600" alt="Antigravity Code Preview">

## Antigravity 2.0 split

At Google I/O 2026 (May 19, 2026) Google split the Antigravity product line into two desktop applications that share the same VS Code-fork shell:

| Product | What it is | Default location (Windows) | macOS | Linux |
|---|---|---|---|---|
| **Antigravity** (2.0) | Standalone agent-orchestration app. **Not a code editor.** | `%LOCALAPPDATA%\Programs\Antigravity\Antigravity.exe` | `/Applications/Antigravity.app` | `/usr/bin/antigravity` |
| **Antigravity IDE** | VS Code fork. The actual code editor. **This is what Unity needs.** | `%LOCALAPPDATA%\Programs\Antigravity IDE\Antigravity IDE.exe` | `/Applications/Antigravity IDE.app` | `/usr/bin/antigravity-ide` |

Versions 1.0.x of this package matched the bare `Antigravity` executable, which after the split is the agent app. On machines that received the 2.0 update, Unity therefore launched the agent app's home screen on every script double-click.

Since version 1.1.0:
- Only Antigravity IDE locations are discovered. The bare `Antigravity` app is never listed in the External Tools dropdown.
- A path chosen with **Browse...** must have an Antigravity IDE file name, and its `resources/app/package.json` manifest `name` is verified, so the agent app is rejected even when selected by hand.
- The dropdown entry is named **Antigravity IDE** (or **Antigravity IDE - Insider**), never the ambiguous **Antigravity**.

### Upgrading from 1.0.x
1. Update the package in the Package Manager (the Git URL has not changed).
2. Make sure [Antigravity IDE](https://antigravity.google/) is installed. The 2.0 agent app cannot be used as a code editor.
3. If your External Script Editor was still set to the bare `Antigravity` app, the package switches it to Antigravity IDE automatically on the next domain reload and logs a message in the Console. If Antigravity IDE cannot be found, a warning is logged instead: install it and select **Antigravity IDE** in **Edit** > **Preferences** > **External Tools**.
4. Double-click a C# script. It should open in a window titled **Antigravity IDE**.

## License

This project is licensed under the [MIT License](LICENSE.md).

Copyright (c) 2025 Termway

### Credits
This project is based on the Unity IDE VSCode package.  
Original License: [com.unity.ide.vscode@1.2 License](https://docs.unity3d.com/Packages/com.unity.ide.vscode@1.2/license/LICENSE.html)