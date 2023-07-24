using HarmonyLib;
using Rewired;

namespace TasTool
{
    [HarmonyPatch(typeof(Player), "GetButton", typeof(int))]
    class Button_Patch
    {
        public static bool Prefix(int actionId, ref bool __result)
        {
            if (Main.TAS.SpecialInput)
                return true;

            switch (actionId)
            {
                case 5: __result = Main.TAS.CurrentFrameInput.Attack; break;
                case 6: __result = Main.TAS.CurrentFrameInput.Jump; break;
                case 8: __result = Main.TAS.CurrentFrameInput.Interact; break;
                case 57: __result = Main.TAS.CurrentFrameInput.RangedAttack; break;
                default: return true;
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(Player), "GetButtonDown", typeof(int))]
    class ButtonDown_Patch
    {
        public static bool Prefix(int actionId, ref bool __result)
        {
            if (Main.TAS.SpecialInput)
                return true;

            switch (actionId)
            {
                case 5: __result = Main.TAS.CurrentFrameInput.Attack; break; // Also check if not pressed last frame
                case 6: __result = Main.TAS.CurrentFrameInput.Jump; break;
                case 8: __result = Main.TAS.CurrentFrameInput.Interact; break;
                case 57: __result = Main.TAS.CurrentFrameInput.RangedAttack; break;
                default: return true;
            }

            return false;
        }
    }

    // GetButtonUp
    // GetButtonPrev

    // GetButtonSinglePressHold
    // GetButtonSinglePressDown
    // GetButtonSinglePressUp

    // GetButtonTimedPress
    // GetButtonTimedPressDown
    // GetButtonTimedPressUp

    // GetButtonShortPress
    // GetButtonShortPressDown
    // GetButtonShortPressUp

    // GetButtonTimePressed
    // GetButtonTimeUnpressed
}
