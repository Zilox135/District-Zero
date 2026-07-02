// AsylumCustomSkyboxes - custom skybox replacement mod for 7 Days To Die
// Author: AsylumMods
// Unauthorized redistribution or rebranding is not permitted.
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SkyboxMaterialLoader
{
    // Attribution signature. Do not remove.
    private const string AUTHOR = "AsylumMods";

    private static Renderer atmosphereRenderer;
    private static Material vanillaAtmosphereMaterial;
    private static Material customSkyboxMaterial;
    private static Material activeMaterial;
    private static readonly List<Cubemap> nightCubemaps = new List<Cubemap>();
    private static bool showingVanilla = true;
    private static bool initialApplyDone = false;

    // Crossfade between night cubemap variants at dusk.
    private const float FADE_TRAIL_HOURS = 2f;
    private static Cubemap pendingCubemap;
    private static int currentNightPeriodDay = int.MinValue;

    public static void LoadCustomSkybox()
    {
        try
        {
            string modFolderPath = Path.Combine(
                Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                "Assets",
                "skybox.unity3d"
            );

            if (!File.Exists(modFolderPath))
            {
                DebugMsg("Skybox file not found at: " + modFolderPath);
                return;
            }

            AssetBundle assetBundle = AssetBundle.LoadFromFile(modFolderPath);
            if (assetBundle == null)
            {
                DebugMsg("Failed to load skybox asset bundle from: " + modFolderPath);
                return;
            }

            Material[] materials = assetBundle.LoadAllAssets<Material>();
            if (materials == null || materials.Length == 0)
            {
                DebugMsg("No materials found in skybox asset bundle");
                assetBundle.Unload(false);
                return;
            }

            customSkyboxMaterial = materials[0];
            if (customSkyboxMaterial == null)
            {
                DebugMsg("Skybox material is null");
                assetBundle.Unload(false);
                return;
            }

            Cubemap[] allCubemaps = assetBundle.LoadAllAssets<Cubemap>();
            LoadNightCubemaps(allCubemaps);
            DebugMsg("Night cubemaps loaded: " + nightCubemaps.Count);

            if (SkyManager.atmosphereSphere == null)
            {
                DebugMsg("SkyManager.atmosphereSphere is not initialized yet");
                assetBundle.Unload(false);
                return;
            }

            atmosphereRenderer = SkyManager.atmosphereSphere.GetComponent<Renderer>();
            if (atmosphereRenderer == null)
            {
                DebugMsg("Atmosphere sphere has no Renderer component");
                assetBundle.Unload(false);
                return;
            }

            vanillaAtmosphereMaterial = SkyManager.atmosphereMtrl;
            if (vanillaAtmosphereMaterial == null)
            {
                DebugMsg("Vanilla atmosphere material is not available");
                assetBundle.Unload(false);
                return;
            }

            MeshFilter mf = SkyManager.atmosphereSphere.GetComponent<MeshFilter>();
            if (mf != null && mf.mesh != null)
                mf.mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 1e6f);

            atmosphereRenderer.enabled = true;
            showingVanilla = true;
            initialApplyDone = false;
            currentNightPeriodDay = int.MinValue;
            pendingCubemap = null;

            GameManager.Instance.StartCoroutine(DayNightLoop());
            DebugMsg("Skybox loader ready — vanilla by day, custom cubemaps at night");

            assetBundle.Unload(false);
        }
        catch (Exception e)
        {
            DebugMsg("Exception loading skybox: " + e.Message + "\n" + e.StackTrace);
        }
    }

    // All bundle cubemaps are night skies; daytime uses vanilla AtmosphereSphere.
    private static void LoadNightCubemaps(Cubemap[] all)
    {
        nightCubemaps.Clear();
        if (all == null) return;
        foreach (Cubemap cm in all)
            if (cm != null) nightCubemaps.Add(cm);
    }

    private static IEnumerator DayNightLoop()
    {
        WaitForSeconds wait = new WaitForSeconds(0.5f);
        while (true)
        {
            ApplySkyForCurrentTime();
            yield return wait;
        }
    }

    private static void ApplySkyForCurrentTime()
    {
        if (atmosphereRenderer == null) return;
        if (GameManager.Instance == null) return;
        World world = GameManager.Instance.World;
        if (world == null) return;
        if (world.worldTime == 0UL) return;

        int dawnHour = world.DawnHour;
        int duskHour = world.DuskHour;
        float hour = (float)(world.worldTime % 24000UL) / 1000f;
        bool isDay = (hour >= dawnHour && hour < duskHour);

        if (isDay)
        {
            if (!showingVanilla)
            {
                atmosphereRenderer.sharedMaterial = vanillaAtmosphereMaterial;
                showingVanilla = true;
                initialApplyDone = false;
                currentNightPeriodDay = int.MinValue;
                pendingCubemap = null;
                activeMaterial = null;
                DebugMsg("Switched to vanilla sky (daytime)");
            }
            return;
        }

        if (showingVanilla)
        {
            atmosphereRenderer.material = customSkyboxMaterial;
            activeMaterial = atmosphereRenderer.material;
            showingVanilla = false;
            initialApplyDone = false;
            currentNightPeriodDay = int.MinValue;
            pendingCubemap = null;
            DebugMsg("Switched to custom night sky");
        }

        if (activeMaterial == null || nightCubemaps.Count == 0) return;
        ApplyNightCubemap(world, hour, duskHour);
    }

    private static void ApplyNightCubemap(World world, float hour, int duskHour)
    {
        int worldDay = world.WorldDay;
        int nightPeriodDay = hour >= duskHour ? worldDay : worldDay - 1;

        if (!initialApplyDone)
        {
            Cubemap c = PickNightVariant(nightPeriodDay);
            if (c == null) return;
            activeMaterial.SetTexture("_Tex", c);
            activeMaterial.SetTexture("_TexB", c);
            activeMaterial.SetFloat("_Blend", 0f);
            currentNightPeriodDay = nightPeriodDay;
            pendingCubemap = null;
            initialApplyDone = true;
            DebugMsg("Initial night cubemap '" + c.name + "' (night period day " + nightPeriodDay + ")");
            return;
        }

        if (nightPeriodDay != currentNightPeriodDay)
        {
            Cubemap next = PickNightVariant(nightPeriodDay);
            if (next != null)
            {
                pendingCubemap = next;
                activeMaterial.SetTexture("_TexB", next);
                activeMaterial.SetFloat("_Blend", 0f);
            }
            currentNightPeriodDay = nightPeriodDay;
            DebugMsg("New night period (day " + nightPeriodDay + "), fading to '" +
                     (next != null ? next.name : "<null>") + "'");
        }

        if (pendingCubemap == null) return;

        float hoursSince = hour >= duskHour
            ? (hour - duskHour)
            : (24f - duskHour + hour);
        if (hoursSince < 0f) hoursSince = 0f;

        if (hoursSince >= FADE_TRAIL_HOURS)
        {
            activeMaterial.SetTexture("_Tex", pendingCubemap);
            activeMaterial.SetFloat("_Blend", 0f);
            pendingCubemap = null;
        }
        else
        {
            float k = Mathf.Clamp01(hoursSince / FADE_TRAIL_HOURS);
            float s = k * k * (3f - 2f * k);
            activeMaterial.SetFloat("_Blend", s);
        }
    }

    private static Cubemap PickNightVariant(int nightPeriodDay)
    {
        if (nightCubemaps.Count == 0) return null;
        int idx = PickVariant(nightPeriodDay, nightCubemaps.Count);
        return nightCubemaps[idx];
    }

    private static int PickVariant(int worldDay, int count)
    {
        if (count <= 1) return 0;
        unchecked
        {
            int seed = worldDay * 397;
            uint h = (uint)seed * 2654435761u;
            return (int)(h % (uint)count);
        }
    }

    private static void DebugMsg(string message)
    {
        Debug.Log("<color=#ff00ff>[" + AUTHOR + "]</color> " + message);
    }
}
