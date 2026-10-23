using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;

/// <summary>
/// Command-line build tool for WE R IN TROUBLE: UI BREAKER.
/// Run via: Unity -batchmode -quit -executeMethod BuildGame.BuildWindows
/// </summary>
public static class BuildGame
{
    public static void BuildWindows()
    {
        string[] scenes = { "Assets/Scenes/Station.unity" };
        string outputPath = "Builds/Windows/TGC_UIBreaker.exe";

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputPath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.Development
        };

        Debug.Log("[BuildGame] Starting Windows build...");
        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[BuildGame] Build SUCCEEDED! Size: {summary.totalSize / 1024 / 1024} MB at {outputPath}");
            System.IO.File.WriteAllText("D:/Work/Unity/TGC/build_result.txt", "SUCCESS\nSize: " + summary.totalSize / 1024 / 1024 + " MB\nPath: " + outputPath);
        }
        else
        {
            Debug.Log($"[BuildGame] Build FAILED! Result: {summary.result}");
            string errors = "";
            foreach (var step in report.steps)
            {
                foreach (var msg in step.messages)
                {
                    if (msg.type == LogType.Error || msg.type == LogType.Exception)
                        errors += msg.content + "\n";
                }
            }
            System.IO.File.WriteAllText("D:/Work/Unity/TGC/build_result.txt", "FAILED\nResult: " + summary.result + "\nErrors:\n" + errors);
        }
    }
}
