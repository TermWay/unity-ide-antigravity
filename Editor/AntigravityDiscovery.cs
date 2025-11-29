using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.CodeEditor;

namespace Antigravity.Editor // <--- NEW NAMESPACE
{
    public interface IDiscovery
    {
        CodeEditor.Installation[] PathCallback();
    }

    public class AntigravityDiscovery : IDiscovery // <--- NEW CLASS NAME
    {
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
            string[] possiblePaths =
#if UNITY_EDITOR_OSX
            {
                "/Applications/Antigravity.app",
                "/Applications/Visual Studio Code.app"
            };
#elif UNITY_EDITOR_WIN
            {
                GetLocalAppData() + @"/Programs/Antigravity/Antigravity.exe",
                GetProgramFiles() + @"/Antigravity/Antigravity.exe",
            };
#else
            {
                "/usr/bin/antigravity",
                "/bin/antigravity"
            };
#endif
            var existingPaths = possiblePaths.Where(AntigravityExists).ToList();
            
            if (existingPaths.Count > 0)
            {
                m_Installations = existingPaths.Select(path => new CodeEditor.Installation
                {
                    Name = "Antigravity", // Always name it Antigravity
                    Path = path
                }).ToList();
            }
        }

#if UNITY_EDITOR_WIN
        static string GetProgramFiles() => Environment.GetEnvironmentVariable("ProgramFiles")?.Replace("\\", "/");
        static string GetLocalAppData() => Environment.GetEnvironmentVariable("LOCALAPPDATA")?.Replace("\\", "/");
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