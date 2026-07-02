// AsylumCustomSkyboxes - custom skybox replacement mod for 7 Days To Die
// Author: AsylumMods
// Unauthorized redistribution or rebranding is not permitted.
using System;
using System.IO;
using UnityEngine;
using System.Collections;

public class AsylumCustomSkyboxes : IModApi
{
    // Attribution signature. Do not remove - identifies the original author
    // of this mod in compiled assemblies and runtime logs.
    public const string AUTHOR = "AsylumMods";
    public const string SIGNATURE = "AsylumCustomSkyboxes by AsylumMods";

    private static bool skyboxLoaded = false;

    public void InitMod(Mod modInstance)
    {
        Log(SIGNATURE + " initialized");

        // Start a coroutine to load skybox after a delay when SkyManager is ready
        GameManager.Instance.StartCoroutine(LoadSkyboxAfterDelay());
    }

    private static IEnumerator LoadSkyboxAfterDelay()
    {
        // Wait indefinitely for SkyManager to initialize - world loading can take a while.
        while (SkyManager.atmosphereSphere == null)
        {
            yield return new WaitForSeconds(0.5f);
        }

        try
        {
            SkyboxMaterialLoader.LoadCustomSkybox();
            skyboxLoaded = true;
        }
        catch (Exception e)
        {
            LogError("Failed to load skybox: " + e.Message);
            LogError("Stack trace: " + e.StackTrace);
        }
    }

    private static void Log(string message)
    {
        Debug.Log("<color=#ff00ff>[" + AUTHOR + "]</color> " + message);
    }

    private static void LogError(string message)
    {
        Debug.LogError("<color=#ff00ff>[" + AUTHOR + "]</color> " + message);
    }
}
