using HarmonyLib;
using RoboticInbox.Utilities;
using System;

namespace RoboticInbox.Patches
{
    [HarmonyPatch(typeof(TEFeatureStorage), nameof(TEFeatureStorage.OnUnlockedServer))]
    internal class TEFeatureStorage_OnUnlockedServer_Patches
    {
        private static readonly ModLog<TEFeatureStorage_OnUnlockedServer_Patches> _log = new ModLog<TEFeatureStorage_OnUnlockedServer_Patches>();

        public static void Postfix(TEFeatureStorage __instance)
        {
            try
            {
                if (!ConnectionManager.Instance.IsServer)
                {
                    return;
                }
                StorageManager.Distribute(__instance.ToWorldPos());
            }
            catch (Exception e)
            {
                _log.Error("Postfix", e);
            }
        }
    }

    [HarmonyPatch(typeof(TEFeatureAbs), nameof(TEFeatureAbs.CanLockOnServer))]
    internal class TEFeatureAbs_CanLockOnServer_Patches
    {
        private static readonly ModLog<TEFeatureAbs_CanLockOnServer_Patches> _log = new ModLog<TEFeatureAbs_CanLockOnServer_Patches>();

        public static bool Prefix(TEFeatureAbs __instance, ref bool __result)
        {
            try
            {
                if (!ConnectionManager.Instance.IsServer)
                {
                    return true; // only gate access on the server
                }
                if (__instance is TEFeatureStorage storage
                    && StorageManager.HasRoboticInboxSecureTag(storage.blockValue.Block)
                    && StorageManager.ActiveCoroutines.ContainsKey(storage.ToWorldPos()))
                {
                    _log.Trace($"[{storage.ToWorldPos()}] robotic inbox denied access because it was actively distributing contents");
                    __result = false;
                    return false; // skip original; deny lock
                }
            }
            catch (Exception e)
            {
                _log.Error("Prefix", e);
            }
            return true;
        }
    }
}
