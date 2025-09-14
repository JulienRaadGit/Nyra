using UnityEditor;
using UnityEngine;

public class OneClickBuild {
    [MenuItem("Build/Build Android & Run %#b")] // Ctrl+Shift+B
    public static void BuildAndRunAndroid() {
        string path = "Builds/Android/Nyra.apk";
        string dir = System.IO.Path.GetDirectoryName(path);
        if (!System.IO.Directory.Exists(dir))
            System.IO.Directory.CreateDirectory(dir);

        BuildPlayerOptions options = new BuildPlayerOptions {
            scenes = new[] { "Assets/Scenes/Main.unity" }, // adapte si ta scène s’appelle autrement
            locationPathName = path,
            target = BuildTarget.Android,
            options = BuildOptions.AutoRunPlayer
        };

        BuildPipeline.BuildPlayer(options);
        Debug.Log("✅ APK built and deployed: " + path);
    }
}
