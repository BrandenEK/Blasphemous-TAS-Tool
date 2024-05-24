using HarmonyLib;
using Rewired;
using UnityEngine;

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

            __result = Main.TAS.CurrentFrameInput.GetInput(actionId) && !Main.TAS.PreviousFrameInput.GetInput(actionId);
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

            __result = !Main.TAS.CurrentFrameInput.GetInput(actionId) && Main.TAS.PreviousFrameInput.GetInput(actionId);
            return false;
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.GetAxis), typeof(int))]
    class Input_GetAxis_Patch
    {
        public static void Postfix(int actionId, ref float __result)
        {
            //Main.TAS.LogWarning($"Checking for axis {actionId}: {__result}");
            if (!Main.TAS.SpecialInput)
            {
                __result = Main.TAS.CurrentFrameInput.GetAxis(actionId);
            }
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.GetAxisRaw), typeof(int))]
    class Input_GetAxisRaw_Patch
    {
        public static void Postfix(int actionId, ref float __result)
        {
            //Main.TAS.LogWarning($"Checking for axis raw {actionId}: {__result}");
            if (!Main.TAS.SpecialInput)
            {
                __result = Main.TAS.CurrentFrameInput.GetAxis(actionId);
            }
        }
    }

    [HarmonyPatch(typeof(Time), nameof(Time.deltaTime), MethodType.Getter)]
    class Time_DeltaTime_Patch
    {
        public static void Postfix(ref float __result)
        {
            __result = Time.timeScale / Main.TAS.FrameRate;
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

    //[HarmonyPatch(typeof(Input), nameof(Input.GetAxis))]
    //class Input_GetAxis_Patch
    //{
    //    public static void Postfix(string axisName, ref float __result)
    //    {
    //        Main.TAS.LogWarning($"Checking axis {axisName}: {__result}");
    //    }
    //}

    //[HarmonyPatch(typeof(Input), nameof(Input.GetButtonDown))]
    //class Input_GetButtonDown_Patch
    //{
    //    public static void Postfix(string buttonName, ref bool __result)
    //    {
    //        Main.TAS.LogWarning($"Checking button {buttonName}: {__result}");
    //    }
    //}

    //[HarmonyPatch(typeof(Input), nameof(Input.GetKeyDownInt))]
    //class Input_GetKeyDownInt_Patch
    //{
    //    public static void Postfix(int key, ref bool __result)
    //    {
    //        Main.TAS.LogWarning($"Checking key {key}: {__result}");
    //    }
    //}
}
