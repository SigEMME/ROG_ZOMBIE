using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace RogZombie.PreGameplayLoop.Editor
{
    [InitializeOnLoad]
    public static class HubWindowsBuild
    {
        private const string Request = ".hub-windows-build.request";
        static HubWindowsBuild() { EditorApplication.update += CheckRequest; }
        private static void CheckRequest()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode || !File.Exists(Request)) return;
            File.Delete(Request);
            Build();
        }

        [MenuItem("ROG ZOMBIE/Pre gameplay loop/Build Windows HUB prototype")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || BuildPipeline.isBuildingPlayer) return;
            string output = Path.GetFullPath("Builds/Windows-HUB-" + DateTime.Now.ToString("yyyyMMdd-HHmmss"));
            Directory.CreateDirectory(output);
            string reportPath = Path.Combine(output, "Build-report.txt");
            try
            {
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = new[] { "Assets/Scenes/HubPrototype.unity" },
                    locationPathName = Path.Combine(output, "ROG_ZOMBIE.exe"),
                    target = BuildTarget.StandaloneWindows64,
                    options = BuildOptions.Development
                });
                var summary = report.summary;
                var text = new StringBuilder();
                text.AppendLine($"Result: {summary.result} | Unity {Application.unityVersion}");
                text.AppendLine($"Errors: {summary.totalErrors} | Warnings: {summary.totalWarnings} | Size: {summary.totalSize} bytes | Time: {summary.totalTime}");
                text.AppendLine("Windows x64 Development | Initial scene: Assets/Scenes/HubPrototype.unity");
                foreach (var step in report.steps)
                    foreach (var message in step.messages)
                        if (message.type == LogType.Warning || message.type == LogType.Error || message.type == LogType.Exception)
                            text.AppendLine(message.type + ": " + message.content);
                File.WriteAllText(reportPath, text.ToString());
                File.WriteAllText("Builds/Latest-HUB-build.txt", output);
                Debug.Log("HUB Windows build: " + summary.result + " — " + output);
            }
            catch (Exception ex)
            {
                File.WriteAllText(reportPath, ex.ToString());
                File.WriteAllText("Builds/Latest-HUB-build.txt", output);
                Debug.LogException(ex);
            }
        }
    }
}
