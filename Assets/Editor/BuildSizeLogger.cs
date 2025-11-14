using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BuildSizeLogger
{
    [MenuItem("Tools/Build WebGL With Detailed Size Report")]
    public static void BuildWebGLWithDetailedSizeReport()
    {
        string buildPath = "Builds/WebGL";

        BuildPlayerOptions opts = new BuildPlayerOptions
        {
            scenes = new[]
            {
                "Assets/Scenes/IntroScene.unity",
                "Assets/Scenes/MainGame.unity"
            },
            locationPathName = buildPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(opts);
        BuildSummary summary = report.summary;

        Debug.Log("========= BUILD REPORT =========");
        Debug.Log($"Result: {summary.result}");
        Debug.Log($"Total Size: {(summary.totalSize / (1024f * 1024f)):F2} MB");
        Debug.Log($"Build Time: {summary.totalTime.TotalSeconds:F1} sec");
        Debug.Log("===============================");

        // 🧩 Variables para desglose
        double textures = 0, shaders = 0, meshes = 0, animations = 0;
        double scripts = 0, dlls = 0, audio = 0, otherAssets = 0;

        // 📦 Analizar assets empacados
        foreach (var packed in report.packedAssets)
        {
            foreach (var content in packed.contents)
            {
                string path = content.sourceAssetPath?.ToLower() ?? "";

                if (path.EndsWith(".png") || path.EndsWith(".jpg") || path.EndsWith(".jpeg"))
                    textures += content.packedSize;
                else if (path.EndsWith(".shader") || path.EndsWith(".cginc"))
                    shaders += content.packedSize;
                else if (path.EndsWith(".dll"))
                    dlls += content.packedSize;
                else if (path.EndsWith(".cs"))
                    scripts += content.packedSize;
                else if (path.EndsWith(".fbx") || path.EndsWith(".obj"))
                    meshes += content.packedSize;
                else if (path.EndsWith(".anim"))
                    animations += content.packedSize;
                else if (path.EndsWith(".wav") || path.EndsWith(".mp3") || path.EndsWith(".ogg"))
                    audio += content.packedSize;
                else
                    otherAssets += content.packedSize;
            }
        }

        // 📊 Mostrar resumen en MB
        Debug.Log("----- Size Breakdown by Category -----");
        Debug.Log($"Textures : {(textures / (1024f * 1024f)):F2} MB");
        Debug.Log($"Shaders  : {(shaders / (1024f * 1024f)):F2} MB");
        Debug.Log($"Meshes   : {(meshes / (1024f * 1024f)):F2} MB");
        Debug.Log($"Animations : {(animations / (1024f * 1024f)):F2} MB");
        Debug.Log($"Scripts (.cs/.dll): {((scripts + dlls) / (1024f * 1024f)):F2} MB");
        Debug.Log($"Audio : {(audio / (1024f * 1024f)):F2} MB");
        Debug.Log($"Other  : {(otherAssets / (1024f * 1024f)):F2} MB");
        Debug.Log("======================================");
    }
}
