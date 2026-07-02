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

    // Day/night cubemap state. The material's _Tex is swapped at dawn/dusk between
    // entries in dayCubemaps / nightCubemaps. activeMaterial is the renderer's
    // instance material we mutate (assigned once when the material is applied).
    private static Material activeMaterial;
    private static readonly List<Cubemap> dayCubemaps = new List<Cubemap>();
    private static readonly List<Cubemap> nightCubemaps = new List<Cubemap>();
    private static bool initialApplyDone = false;

    // Crossfade state. _Tex is the cubemap of the period we are leaving; _TexB
    // is the cubemap of the period we are entering. _Blend is driven directly
    // from the in-game time-of-day so the fade lines up with the dusk/dawn
    // boundary regardless of day length, time skips, or save reloads.
    //
    // The fade starts AT each dusk/dawn boundary and ends FADE_TRAIL_HOURS
    // in-game hours later. This keeps the sky matching the daylight period
    // through the vanilla sunset/sunrise lighting transition, and only shifts
    // to the new cubemap after the world's own ambient lighting has finished
    // its swing. _TexB is then promoted to _Tex and _Blend reset to 0.
    private const float FADE_TRAIL_HOURS = 2f;
    private static Cubemap currentCubemap;
    private static Cubemap pendingCubemap;
    private static int currentPeriodDay = int.MinValue;
    private static bool currentIsDay;

    public static void LoadCustomSkybox()
    {
        try
        {
            // Build the path to the skybox.unity3d file in the mod folder
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

            Material skyboxMaterial = materials[0];
            if (skyboxMaterial == null)
            {
                DebugMsg("Skybox material is null");
                assetBundle.Unload(false);
                return;
            }

            // Load every cubemap in the bundle and partition into day/night pools
            // for runtime swapping.
            Cubemap[] allCubemaps = assetBundle.LoadAllAssets<Cubemap>();
            PartitionCubemaps(allCubemaps);
            DebugMsg("Cubemaps loaded: total=" + (allCubemaps != null ? allCubemaps.Length : 0) +
                     " day=" + dayCubemaps.Count + " night=" + nightCubemaps.Count);

            // Apply to the atmosphere sphere in SkyManager
            if (SkyManager.atmosphereSphere != null)
            {
                Transform sphereT = SkyManager.atmosphereSphere;
                Renderer renderer = sphereT.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = skyboxMaterial;
                    renderer.enabled = true;

                    // Force the sphere's mesh bounds huge so it cannot be frustum-culled.
                    MeshFilter mf = sphereT.GetComponent<MeshFilter>();
                    if (mf != null && mf.mesh != null)
                        mf.mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 1e6f);

                    // Capture the renderer's instance material so future _Tex swaps
                    // mutate the correct object (not the shared asset).
                    activeMaterial = renderer.material;

                    // Start the day/night switcher.
                    GameManager.Instance.StartCoroutine(DayNightLoop());

                    DebugMsg("Custom skybox material applied");
                }
                else
                {
                    DebugMsg("Atmosphere sphere has no Renderer component");
                }
            }
            else
            {
                DebugMsg("SkyManager.atmosphereSphere is not initialized yet");
            }

            // Hide the vanilla clouds dome so it does not draw over the custom sky
            if (SkyManager.cloudsSphere != null)
            {
                Renderer cloudsRenderer = SkyManager.cloudsSphere.GetComponent<Renderer>();
                if (cloudsRenderer != null)
                {
                    cloudsRenderer.enabled = false;
                    DebugMsg("Vanilla clouds dome hidden");
                }
            }

            // Hide the vanilla moon sprite. It is repositioned every frame from
            // worldRotation which drags it across the sky; some of our night
            // cubemaps already have a moon baked in, so the rotating disc would
            // double-up and look wrong.
            if (SkyManager.moonSpriteT != null)
            {
                Renderer moonRenderer = SkyManager.moonSpriteT.GetComponent<Renderer>();
                if (moonRenderer != null)
                {
                    moonRenderer.enabled = false;
                    DebugMsg("Vanilla moon sprite hidden");
                }
            }

            // Unload the asset bundle (materials are already loaded into memory)
            assetBundle.Unload(false);
        }
        catch (Exception e)
        {
            DebugMsg("Exception loading skybox: " + e.Message + "\n" + e.StackTrace);
        }
    }
    
    // Partition every cubemap in the bundle into day/night pools. Detection order:
    //   1. Name contains "night" / starts with n_ / n-       -> night
    //   2. Name contains "day"   / starts with d_ / d-       -> day
    //   3. Anything left over: distributed evenly. If nothing was labeled at all,
    //      the full list is split alphabetically (first half day, second half night).
    private static void PartitionCubemaps(Cubemap[] all)
    {
        dayCubemaps.Clear();
        nightCubemaps.Clear();
        if (all == null || all.Length == 0) return;

        List<Cubemap> unlabeled = new List<Cubemap>();
        foreach (Cubemap cm in all)
        {
            if (cm == null) continue;
            string n = cm.name.ToLowerInvariant();
            if (n.Contains("night") || n.StartsWith("n_") || n.StartsWith("n-"))
                nightCubemaps.Add(cm);
            else if (n.Contains("day") || n.StartsWith("d_") || n.StartsWith("d-"))
                dayCubemaps.Add(cm);
            else
                unlabeled.Add(cm);
        }

        if (dayCubemaps.Count == 0 && nightCubemaps.Count == 0)
        {
            unlabeled.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase));
            int half = (unlabeled.Count + 1) / 2;
            for (int i = 0; i < unlabeled.Count; i++)
            {
                if (i < half) dayCubemaps.Add(unlabeled[i]);
                else nightCubemaps.Add(unlabeled[i]);
            }
        }
        else
        {
            foreach (Cubemap cm in unlabeled)
            {
                if (dayCubemaps.Count <= nightCubemaps.Count) dayCubemaps.Add(cm);
                else nightCubemaps.Add(cm);
            }
        }
    }

    // Polls every 0.5 seconds. The blend value is recomputed from the in-game
    // time-of-day each tick, so the visible fade is always in sync with the
    // game clock - no separate timer to drift, and time skips / save loads
    // resolve to the correct blend instantly.
    private static IEnumerator DayNightLoop()
    {
        WaitForSeconds wait = new WaitForSeconds(0.5f);
        initialApplyDone = false;
        while (true)
        {
            ApplyCubemapForCurrentTime();
            yield return wait;
        }
    }

    private static void ApplyCubemapForCurrentTime()
    {
        if (activeMaterial == null) return;
        if (GameManager.Instance == null) return;
        World world = GameManager.Instance.World;
        if (world == null) return;

        // Skip while the world hasn't fully loaded its save data.
        if (world.worldTime == 0UL) return;

        int worldDay = world.WorldDay;
        int duskHour = world.DuskHour;
        int dawnHour = world.DawnHour;
        // Fractional hour in [0, 24). worldTime advances 1000 units per in-game hour.
        float hour = (float)(world.worldTime % 24000UL) / 1000f;

        bool isDay = (hour >= dawnHour && hour < duskHour);
        // A single night spans midnight, so tie it to the day it STARTED on.
        int periodDay = isDay
            ? worldDay
            : (hour >= duskHour ? worldDay : worldDay - 1);

        // First-time init: snap to the current period's cubemap, no fade.
        if (!initialApplyDone)
        {
            Cubemap c = PickForPeriod(isDay, periodDay);
            if (c == null) return;
            activeMaterial.SetTexture("_Tex", c);
            activeMaterial.SetTexture("_TexB", c);
            activeMaterial.SetFloat("_Blend", 0f);
            currentCubemap = c;
            currentIsDay = isDay;
            currentPeriodDay = periodDay;
            pendingCubemap = null;
            initialApplyDone = true;
            DebugMsg("Initial " + (isDay ? "DAY" : "NIGHT") + " cubemap '" + c.name +
                     "' (period day " + periodDay + ")");
            return;
        }

        // Period change detected: the boundary just crossed. Start a fade FROM
        // the previously-displayed cubemap (still in _Tex) TO the new period's
        // cubemap, which goes into _TexB. _Blend remains at 0 for this first
        // tick - the time-driven ramp below takes over.
        if (isDay != currentIsDay || periodDay != currentPeriodDay)
        {
            Cubemap next = PickForPeriod(isDay, periodDay);
            if (next != null)
            {
                pendingCubemap = next;
                activeMaterial.SetTexture("_TexB", next);
                activeMaterial.SetFloat("_Blend", 0f);
            }
            currentIsDay = isDay;
            currentPeriodDay = periodDay;
            DebugMsg("Entered " + (isDay ? "DAY" : "NIGHT") + " (period day " + periodDay +
                     "), fading to '" + (next != null ? next.name : "<null>") + "'");
        }

        // No fade pending - nothing to do.
        if (pendingCubemap == null) return;

        // In-game hours since the most recent dusk/dawn boundary.
        float hoursSince;
        if (isDay)
            hoursSince = hour - dawnHour;                                  // since dawn today
        else
            hoursSince = (hour >= duskHour) ? (hour - duskHour)            // since dusk today
                                            : (24f - duskHour + hour);     // since dusk yesterday
        if (hoursSince < 0f) hoursSince = 0f;

        if (hoursSince >= FADE_TRAIL_HOURS)
        {
            // Fade complete. Promote _TexB to _Tex and reset blend.
            activeMaterial.SetTexture("_Tex", pendingCubemap);
            activeMaterial.SetFloat("_Blend", 0f);
            currentCubemap = pendingCubemap;
            pendingCubemap = null;
        }
        else
        {
            float k = Mathf.Clamp01(hoursSince / FADE_TRAIL_HOURS);
            // Smoothstep so the visible motion eases in/out instead of being linear.
            float s = k * k * (3f - 2f * k);
            activeMaterial.SetFloat("_Blend", s);
        }
    }

    // Picks the deterministic variant for a given period, with a fallback to
    // the opposite pool if one side is empty.
    private static Cubemap PickForPeriod(bool isDay, int periodDay)
    {
        List<Cubemap> pool = isDay ? dayCubemaps : nightCubemaps;
        if (pool.Count == 0)
        {
            pool = isDay ? nightCubemaps : dayCubemaps;
            if (pool.Count == 0) return null;
        }
        int idx = PickVariant(periodDay, isDay, pool.Count);
        return pool[idx];
    }

    // Variant selection. Change the body to swap strategies:
    //   SeededByDay (default) - deterministic per save: same world day + period
    //                           always returns the same variant. Stable across reloads.
    //   Cycle                 - idx = worldDay % count. Predictable rotation.
    //   Random                - new pick every transition. Most variety, least stable.
    private static int PickVariant(int worldDay, bool isDay, int count)
    {
        if (count <= 1) return 0;
        // SeededByDay
        unchecked
        {
            int seed = (worldDay * 397) ^ (isDay ? 0x55555555 : 0x2AAAAAAA);
            uint h = (uint)seed * 2654435761u;
            return (int)(h % (uint)count);
        }
    }

    private static void DebugMsg(string message)
    {
        Debug.Log("<color=#ff00ff>[" + AUTHOR + "]</color> " + message);
    }
}
