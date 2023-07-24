using HarmonyLib;
using Rewired;

namespace TasTool
{
    [HarmonyPatch(typeof(Player), "GetButtonDown", typeof(int))]
    class ButtonDown_Patch
    {
        public static bool Prefix(int actionId, ref bool __result)
        {
            if (Main.TAS.SpecialInput)
                return true;

            if (actionId == 6)
            {
                __result = Main.TAS.CurrentFrameInput.Jump; // Check if frame before is not pressed
                return false;
            }
            else if (actionId == 57)
            {
                __result = Main.TAS.CurrentFrameInput.RangedAttack;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Player), "GetButton", typeof(int))]
    class Button_Patch
    {
        public static bool Prefix(int actionId, ref bool __result)
        {
            if (Main.TAS.SpecialInput)
                return true;

            if (actionId == 6)
            {
                __result = Main.TAS.CurrentFrameInput.Jump;
                return false;
            }
            else if (actionId == 57)
            {
                __result = Main.TAS.CurrentFrameInput.RangedAttack;
                return false;
            }

            return true;
        }
    }
}
