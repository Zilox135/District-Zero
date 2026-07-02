using System.IO;
using UnityEditor;
using UnityEngine;

// Replacement for MultiPlatformExportAssetBundles.cs which uses the obsolete
// BuildPipeline.BuildAssetBundle (singular) API and crashes with
// "Assertion failed on expression: 'm_ManagersToReset.empty()'" in Unity 2022.3.
//
// This script uses the modern BuildPipeline.BuildAssetBundles (plural) API.
// It builds every asset that has an AssetBundle name tag set in the Inspector,
// then renames "skybox" to "skybox.unity3d" to match the loader path expected
// by SkyboxMaterialLoader.cs in the 7DTD CustomSkybox mod.

public static class BuildSkyboxBundle
{
    private const string OutputFolder    = "BuiltBundles";
    private const string BundleName      = "skybox";
    private const string OutputExtension = ".unity3d";

    [MenuItem("7DTD/Build Skybox Bundle (Windows64) - LZ4")]
    public static void BuildLZ4()
    {
        Build(BuildAssetBundleOptions.ChunkBasedCompression);
    }

    [MenuItem("7DTD/Build Skybox Bundle (Windows64) - Uncompressed")]
    public static void BuildUncompressed()
    {
        Build(BuildAssetBundleOptions.UncompressedAssetBundle);
    }

    private static void Build(BuildAssetBundleOptions options)
    {
        string outDir = Path.Combine(Application.dataPath, "..", OutputFolder);
        Directory.CreateDirectory(outDir);

        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(
            outDir,
            options,
            BuildTarget.StandaloneWindows64);

        if (manifest == null)
        {
            Debug.LogError("[BuildSkyboxBundle] Build failed - see console for errors.");
            return;
        }

        string src = Path.Combine(outDir, BundleName);
        string dst = Path.Combine(outDir, BundleName + OutputExtension);

        if (!File.Exists(src))
        {
            Debug.LogError("[BuildSkyboxBundle] Expected bundle '" + BundleName +
                "' was not produced. Make sure your material's AssetBundle name " +
                "(bottom of the Inspector) is set to '" + BundleName + "'.");
            return;
        }

        if (File.Exists(dst)) File.Delete(dst);
        File.Move(src, dst);

        FileInfo info = new FileInfo(dst);
        Debug.Log("[BuildSkyboxBundle] Built: " + dst + " (" + info.Length + " bytes)");
        EditorUtility.RevealInFinder(dst);
    }
}
