using HarmonyLib;
using System.Reflection;

/// <summary>
/// Vanilla SmellCountItems crashes when an inventory slot has a null ItemClass (common with mod items).
/// This keeps smell fully functional and skips bad slots instead of throwing every tick.
/// </summary>
[HarmonyPatch(typeof(PlayerStealth), "SmellCountItems")]
public class PlayerStealth_SmellCountItems_Patch
{
    private static readonly FieldInfo PlayerField = AccessTools.Field(typeof(PlayerStealth), "player");
    private static readonly FieldInfo PlayerLocalField = AccessTools.Field(typeof(PlayerStealth), "playerLocal");

    static bool Prefix(ref PlayerStealth __instance, ref int __result)
    {
        var player = (EntityPlayer)PlayerField.GetValue(__instance);
        var playerLocal = (EntityPlayerLocal)PlayerLocalField.GetValue(__instance);
        float smellTotal = 0f;

        if (playerLocal != null && playerLocal.PlayerUI != null && playerLocal.PlayerUI.xui != null
            && playerLocal.PlayerUI.xui.DragAndDropWindow != null)
        {
            ItemStack currentStack = playerLocal.PlayerUI.xui.DragAndDropWindow.CurrentStack;
            if (!currentStack.IsEmpty())
            {
                smellTotal += GetStackSmell(currentStack);
            }
        }

        if (player != null && player.inventory != null)
        {
            Inventory inventory = player.inventory;
            int slotCount = inventory.GetSlotCount();
            for (int i = 0; i < slotCount; i++)
            {
                ItemStack itemStack = inventory.GetItemStack(i);
                if (itemStack != null && itemStack.count > 0)
                {
                    smellTotal += GetStackSmell(itemStack);
                }
            }
        }

        if (player != null && player.bag != null)
        {
            ItemStack[] slots = player.bag.GetSlots();
            if (slots != null)
            {
                for (int i = 0; i < slots.Length; i++)
                {
                    ItemStack itemStack = slots[i];
                    if (itemStack != null && itemStack.count > 0)
                    {
                        smellTotal += GetStackSmell(itemStack);
                    }
                }
            }
        }

        __result = (int)Utils.FastMin(smellTotal, 50f);
        return false;
    }

    private static float GetStackSmell(ItemStack stack)
    {
        if (stack.itemValue.IsEmpty())
        {
            return 0f;
        }

        ItemClass itemClass = stack.itemValue.ItemClass;
        return itemClass == null ? 0f : itemClass.Smell * stack.count;
    }
}
