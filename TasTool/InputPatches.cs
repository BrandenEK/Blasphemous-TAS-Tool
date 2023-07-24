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

            __result = Main.TAS.CurrentFrameInput.GetInput(actionId);
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

            __result = Main.TAS.CurrentFrameInput.GetInput(actionId);
            return false;
        }
    }

    [HarmonyPatch(typeof(Player), "GetButtonUp", typeof(int))]
    class ButtonUp_Patch
    {
        public static bool Prefix(int actionId, ref bool __result)
        {
            if (Main.TAS.SpecialInput)
                return true;

            __result = !Main.TAS.CurrentFrameInput.GetInput(actionId);
            return false;
        }
    }

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
