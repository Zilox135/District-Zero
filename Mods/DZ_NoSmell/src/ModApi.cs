using System.Reflection;
using HarmonyLib;

public class DZNoSmellModApi : IModApi
{
    public void InitMod(Mod _modInstance)
    {
        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), "DZNoSmell");
    }
}
