using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.CodeEditor;
using UnityEngine;

namespace Antigravity.Editor // <--- NEW NAMESPACE
{
    public interface IDiscovery
    {
        CodeEditor.Installation[] PathCallback();
    }

    /// <summary>
    /// Discovers and validates Antigravity IDE installations.
    ///
    /// Since Antigravity 2.0 (announced at Google I/O on 2026-05-19) the product line is split
    /// into two desktop applications that share the same VS Code-fork shell:
    ///   - "Antigravity"     : the standalone agent-orchestration app. It has no code editor.
    ///   - "Antigravity IDE" : the VS Code fork that actually edits code.
    /// Only Antigravity IDE can act as a Unity external script editor, so the bare "Antigravity"
    /// executable is never offered in the External Tools dropdown and is rejected when selected
    /// manually. The two apps are told apart by their file name and, as a second line of defence,
    /// by the "name" field of their "resources/app/package.json" manifest.
    /// </summary>
    public class AntigravityDiscovery : IDiscovery // <--- NEW CLASS NAME
    {
        const string k_IdeDisplayName = "Antigravity IDE";

        // File name of the IDE, lower-cased with spaces, dashes and underscores removed.
        // Matches "Antigravity IDE.exe", "Antigravity IDE - Insider.app", "antigravity-ide-insiders", ...
        const string k_IdeNormalizedName = "antigravityide";

        // File name of the standalone agent app (and of the IDE before the 2.0 split), normalized the same way.
        const string k_AgentAppNormalizedName = "antigravity";

        static readonly string[] k_SupportedExtensions = { "", ".exe", ".cmd", ".app" };

        List<CodeEditor.Installation> m_Installations;

        public CodeEditor.Installation[] PathCallback()
        {
            if (m_Installations == null)
            {
                m_Installations = new List<CodeEditor.Installation>();
                FindInstallationPaths();
            }

            return m_Installations.ToArray();
        }

        void FindInstallationPaths()
        {
            m_Installations = GetCandidatePaths()
                .Distinct()
                .Where(AntigravityExists)
                .Where(IsAntigravityIde)
                .Select(path => new CodeEditor.Installation
                {
                    Name = GetInstallationName(path),
                    Path = path
                })
                .ToList();
        }

        /// <summary>
        /// Well-known Antigravity IDE install locations. The bare "Antigravity" locations are
        /// intentionally left out: they now host the agent app, not the editor.
        /// </summary>
        static IEnumerable<string> GetCandidatePaths()
        {
            var candidates = new List<string>();
#if UNITY_EDITOR_OSX
            candidates.AddRange(EnumerateAppBundles("/Applications"));
            candidates.AddRange(EnumerateAppBundles(GetUserProfile() + "/Applications"));
#elif UNITY_EDITOR_WIN
            foreach (var baseDirectory in new[] { GetLocalAppData() + "/Programs", GetProgramFiles(), GetProgramFilesX86() })
            {
                if (string.IsNullOrEmpty(baseDirectory))
                    continue;

                candidates.Add(baseDirectory + "/Antigravity IDE/Antigravity IDE.exe");
                candidates.Add(baseDirectory + "/Antigravity IDE - Insider/Antigravity IDE - Insider.exe");
                candidates.Add(baseDirectory + "/Google/Antigravity IDE/Antigravity IDE.exe");
            }
#else
            foreach (var binDirectory in new[] { "/usr/bin", "/bin", "/usr/local/bin", "/snap/bin", GetUserProfile() + "/.local/bin" })
            {
                candidates.Add(binDirectory + "/antigravity-ide");
                candidates.Add(binDirectory + "/antigravity-ide-insiders");
            }
#endif
            return candidates;
        }

        /// <summary>
        /// Returns true when <paramref name="path"/> points to an Antigravity IDE executable or app bundle.
        /// The standalone "Antigravity" agent app is rejected.
        /// </summary>
        public static bool IsAntigravityIde(string path)
        {
            if (!HasAntigravityIdeFileName(path))
                return false;

            // Both apps share the same Code-fork layout; the manifest "name" is the reliable way to
            // tell them apart ("Antigravity" vs "Antigravity IDE"). When it cannot be read (unusual
            // layout, symlink, path does not exist yet) the file name check above has to be enough.
            var manifestName = TryReadManifestName(path);
            if (string.IsNullOrEmpty(manifestName))
                return true;

            return Normalize(manifestName).Contains(k_IdeNormalizedName);
        }

        /// <summary>
        /// Returns true when <paramref name="path"/> points to the bare "Antigravity" application:
        /// the standalone agent app since Antigravity 2.0, and what this package used to register
        /// as the editor before the split.
        /// </summary>
        public static bool IsStandaloneAntigravityApp(string path)
        {
            if (!TryGetNormalizedFileName(path, out var normalizedName))
                return false;

            return normalizedName == k_AgentAppNormalizedName
                || normalizedName == k_AgentAppNormalizedName + "insider"
                || normalizedName == k_AgentAppNormalizedName + "insiders";
        }

        /// <summary>
        /// Name shown in the External Tools dropdown for the given installation path.
        /// </summary>
        public static string GetInstallationName(string path)
        {
            return TryGetNormalizedFileName(path, out var normalizedName) && normalizedName.Contains("insider")
                ? k_IdeDisplayName + " - Insider"
                : k_IdeDisplayName;
        }

        static bool HasAntigravityIdeFileName(string path)
        {
            if (!TryGetNormalizedFileName(path, out var normalizedName))
                return false;

            return normalizedName.StartsWith(k_IdeNormalizedName, StringComparison.Ordinal);
        }

        // "C:/.../Antigravity IDE - Insider.exe" -> "antigravityideinsider", "/usr/bin/antigravity-ide" -> "antigravityide"
        static bool TryGetNormalizedFileName(string path, out string normalizedName)
        {
            normalizedName = null;
            if (string.IsNullOrEmpty(path))
                return false;

            var fileName = Path.GetFileName(path.Replace('\\', '/').TrimEnd('/'));
            if (string.IsNullOrEmpty(fileName))
                return false;

            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (!k_SupportedExtensions.Contains(extension))
                return false;

            normalizedName = Normalize(Path.GetFileNameWithoutExtension(fileName));
            return normalizedName.Length > 0;
        }

        static string Normalize(string value)
        {
            return value.ToLowerInvariant().Replace(" ", "").Replace("-", "").Replace("_", "");
        }

        // Only the fields we read from "resources/app/package.json"; populated by JsonUtility.
#pragma warning disable 649
        [Serializable]
        class AppManifest
        {
            public string name;
        }
#pragma warning restore 649

        static string TryReadManifestName(string path)
        {
            try
            {
                foreach (var manifestPath in GetManifestCandidates(path))
                {
                    if (!File.Exists(manifestPath))
                        continue;

                    var manifest = JsonUtility.FromJson<AppManifest>(File.ReadAllText(manifestPath));
                    return manifest?.name;
                }
            }
            catch (Exception)
            {
                // An unreadable manifest must not prevent the editor from being used.
            }

            return null;
        }

        static IEnumerable<string> GetManifestCandidates(string path)
        {
            var fullPath = path.Replace('\\', '/').TrimEnd('/');
#if UNITY_EDITOR_OSX
            // "Antigravity IDE.app" is a directory: the manifest lives inside the bundle.
            yield return fullPath + "/Contents/Resources/app/package.json";
#else
            var directory = Path.GetDirectoryName(fullPath);
            if (string.IsNullOrEmpty(directory))
                yield break;

            // "<install>/Antigravity IDE.exe" -> "<install>/resources/app/package.json"
            yield return directory + "/resources/app/package.json";

            // "<install>/bin/antigravity-ide" -> "<install>/resources/app/package.json"
            var parent = Path.GetDirectoryName(directory);
            if (!string.IsNullOrEmpty(parent))
                yield return parent + "/resources/app/package.json";
#if UNITY_EDITOR_LINUX
            // "/usr/bin/antigravity-ide" is usually a symlink into "/usr/share/antigravity-ide/".
            var fileName = Path.GetFileName(fullPath);
            yield return "/usr/share/" + fileName + "/resources/app/package.json";
            yield return "/opt/" + fileName + "/resources/app/package.json";
#endif
#endif
        }

#if UNITY_EDITOR_OSX
        static IEnumerable<string> EnumerateAppBundles(string directory)
        {
            try
            {
                if (Directory.Exists(directory))
                    return Directory.GetDirectories(directory, "Antigravity IDE*.app");
            }
            catch (Exception)
            {
                // Unreadable directory: nothing to discover there.
            }

            return Enumerable.Empty<string>();
        }
#endif

#if UNITY_EDITOR_WIN
        static string GetProgramFiles() => Environment.GetEnvironmentVariable("ProgramFiles")?.Replace("\\", "/");
        static string GetProgramFilesX86() => Environment.GetEnvironmentVariable("ProgramFiles(x86)")?.Replace("\\", "/");
        static string GetLocalAppData() => Environment.GetEnvironmentVariable("LOCALAPPDATA")?.Replace("\\", "/");
#else
        static string GetUserProfile() => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).Replace("\\", "/");
#endif

        static bool AntigravityExists(string path)
        {
#if UNITY_EDITOR_OSX
            return Directory.Exists(path);
#else
            return File.Exists(path);
#endif
        }
    }
}
