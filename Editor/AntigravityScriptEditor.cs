using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Unity.CodeEditor;

namespace Antigravity.Editor
{
    [InitializeOnLoad]
    public class AntigravityScriptEditor : IExternalCodeEditor
    {
        const string antigravity_argument = "antigravity_arguments"; // Unique pref keys
        const string antigravity_extension = "antigravity_userExtensions";

        static readonly GUIContent k_ResetArguments = EditorGUIUtility.TrTextContent("Reset argument");
        string m_Arguments;

        IDiscovery m_Discoverability;
        IGenerator m_ProjectGeneration;

        // Logged at most once per editor session when the agent app is selected but no IDE is installed.
        const string k_MissingIdeWarningKey = "antigravity_ide_missing_warning_shown";

        static bool IsOSX => Application.platform == RuntimePlatform.OSXEditor;
        static string DefaultApp => EditorPrefs.GetString("kScriptsDefaultApp");
        static string DefaultArgument { get; } = "\"$(ProjectPath)\" -g \"$(File)\":$(Line):$(Column)";

        string Arguments
        {
            get => m_Arguments ?? (m_Arguments = EditorPrefs.GetString(antigravity_argument, DefaultArgument));
            set
            {
                m_Arguments = value;
                EditorPrefs.SetString(antigravity_argument, value);
            }
        }

        static string[] defaultExtensions
        {
            get
            {
                var customExtensions = new[] { "json", "asmdef", "log" };
                return EditorSettings.projectGenerationBuiltinExtensions
                    .Concat(EditorSettings.projectGenerationUserExtensions)
                    .Concat(customExtensions)
                    .Distinct().ToArray();
            }
        }

        static string[] HandledExtensions
        {
            get
            {
                return HandledExtensionsString
                    .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.TrimStart('.', '*'))
                    .ToArray();
            }
        }

        static string HandledExtensionsString
        {
            get => EditorPrefs.GetString(antigravity_extension, string.Join(";", defaultExtensions));
            set => EditorPrefs.SetString(antigravity_extension, value);
        }

        public bool TryGetInstallationForPath(string editorPath, out CodeEditor.Installation installation)
        {
            // Only Antigravity IDE is accepted. Since Antigravity 2.0 the bare "Antigravity" executable
            // is the standalone agent app, which has no code editor, so it must not be registered here.
            if (!AntigravityDiscovery.IsAntigravityIde(editorPath))
            {
                installation = default;
                return false;
            }

            installation = new CodeEditor.Installation
            {
                Name = AntigravityDiscovery.GetInstallationName(editorPath),
                Path = editorPath
            };

            return true;
        }

        public void OnGUI()
        {
            Arguments = EditorGUILayout.TextField("External Script Editor Args", Arguments);
            if (GUILayout.Button(k_ResetArguments, GUILayout.Width(120)))
            {
                Arguments = DefaultArgument;
            }

            EditorGUILayout.LabelField("Generate .csproj files for:");
            EditorGUI.indentLevel++;
            SettingsButton(ProjectGenerationFlag.Embedded, "Embedded packages", "");
            SettingsButton(ProjectGenerationFlag.Local, "Local packages", "");
            SettingsButton(ProjectGenerationFlag.Registry, "Registry packages", "");
            SettingsButton(ProjectGenerationFlag.Git, "Git packages", "");
            SettingsButton(ProjectGenerationFlag.BuiltIn, "Built-in packages", "");
            SettingsButton(ProjectGenerationFlag.Unknown, "Packages from unknown sources", "");
            RegenerateProjectFiles();
            EditorGUI.indentLevel--;
        }

        void RegenerateProjectFiles()
        {
            var rect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(new GUILayoutOption[] { }));
            rect.width = 252;
            if (GUI.Button(rect, "Regenerate project files"))
            {
                m_ProjectGeneration.Sync();
            }
        }

        void SettingsButton(ProjectGenerationFlag preference, string guiMessage, string toolTip)
        {
            var prevValue = m_ProjectGeneration.AssemblyNameProvider.ProjectGenerationFlag.HasFlag(preference);
            var newValue = EditorGUILayout.Toggle(new GUIContent(guiMessage, toolTip), prevValue);
            if (newValue != prevValue)
            {
                m_ProjectGeneration.AssemblyNameProvider.ToggleProjectGeneration(preference);
            }
        }

        public void CreateIfDoesntExist()
        {
            if (!m_ProjectGeneration.SolutionExists())
            {
                m_ProjectGeneration.Sync();
            }
        }

        public void SyncIfNeeded(string[] addedFiles, string[] deletedFiles, string[] movedFiles, string[] movedFromFiles, string[] importedFiles)
        {
            (m_ProjectGeneration.AssemblyNameProvider as IPackageInfoCache)?.ResetPackageInfoCache();
            m_ProjectGeneration.SyncIfNeeded(addedFiles.Union(deletedFiles).Union(movedFiles).Union(movedFromFiles).ToList(), importedFiles);
        }

        public void SyncAll()
        {
            (m_ProjectGeneration.AssemblyNameProvider as IPackageInfoCache)?.ResetPackageInfoCache();
            AssetDatabase.Refresh();
            m_ProjectGeneration.Sync();
        }

        public bool OpenProject(string path, int line, int column)
        {
            if (path != "" && (!SupportsExtension(path) || !File.Exists(path)))
            {
                return false;
            }

            if (line == -1) line = 1;
            if (column == -1) column = 0;

            string arguments;
            if (Arguments != DefaultArgument)
            {
                arguments = m_ProjectGeneration.ProjectDirectory != path
                    ? CodeEditor.ParseArgument(Arguments, path, line, column)
                    : m_ProjectGeneration.ProjectDirectory;
            }
            else
            {
                arguments = $@"""{m_ProjectGeneration.ProjectDirectory}""";
                if (m_ProjectGeneration.ProjectDirectory != path && path.Length != 0)
                {
                    arguments += $@" -g ""{path}"":{line}:{column}";
                }
            }

            if (IsOSX) return OpenOSX(arguments);

            var app = DefaultApp;
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = app,
                    Arguments = arguments,
                    WindowStyle = app.EndsWith(".cmd", StringComparison.OrdinalIgnoreCase) ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal,
                    CreateNoWindow = true,
                    UseShellExecute = true,
                }
            };

            process.Start();
            return true;
        }

        static bool OpenOSX(string arguments)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "open",
                    Arguments = $"-n \"{DefaultApp}\" --args {arguments}",
                    UseShellExecute = true,
                }
            };
            process.Start();
            return true;
        }

        static bool SupportsExtension(string path)
        {
            var extension = Path.GetExtension(path);
            if (string.IsNullOrEmpty(extension)) return false;
            return HandledExtensions.Contains(extension.TrimStart('.'));
        }

        public CodeEditor.Installation[] Installations => m_Discoverability.PathCallback();

        public AntigravityScriptEditor(IDiscovery discovery, IGenerator projectGeneration)
        {
            m_Discoverability = discovery;
            m_ProjectGeneration = projectGeneration;
        }

        static AntigravityScriptEditor()
        {
            var editor = new AntigravityScriptEditor(new AntigravityDiscovery(), new ProjectGeneration(Directory.GetParent(Application.dataPath).FullName));
            CodeEditor.Register(editor);

            var currentEditorPath = CodeEditor.CurrentEditorInstallation;
            if (AntigravityDiscovery.IsAntigravityIde(currentEditorPath))
            {
                editor.CreateIfDoesntExist();
            }
            else if (AntigravityDiscovery.IsStandaloneAntigravityApp(currentEditorPath))
            {
                // Selected before the Antigravity 2.0 split (or picked by hand): "Antigravity" is now the
                // agent app, not the editor. Unity would otherwise keep launching it for every script.
                EditorApplication.delayCall += () => editor.MigrateToAntigravityIde(currentEditorPath);
            }
        }

        void MigrateToAntigravityIde(string previousEditorPath)
        {
            var ide = Installations.FirstOrDefault();
            if (string.IsNullOrEmpty(ide.Path))
            {
                if (!SessionState.GetBool(k_MissingIdeWarningKey, false))
                {
                    SessionState.SetBool(k_MissingIdeWarningKey, true);
                    UnityEngine.Debug.LogWarning(
                        $"[Antigravity IDE] The selected External Script Editor '{previousEditorPath}' is the standalone Antigravity agent app, " +
                        "which no longer contains a code editor. Install Antigravity IDE (https://antigravity.google/) and select it in " +
                        "Edit > Preferences > External Tools.");
                }
                return;
            }

            CodeEditor.SetExternalScriptEditor(ide.Path);
            UnityEngine.Debug.Log($"[Antigravity IDE] External Script Editor switched from '{previousEditorPath}' (Antigravity agent app) to '{ide.Path}'.");
            CreateIfDoesntExist();
        }

        public void Initialize(string editorInstallationPath) { }
    }
}