using Harmony;
using MelonLoader;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine.XR;

namespace BTKSAGestureMod
{
    public static class BuildInfo
    {
        public const string Name = "BTKSAGestureMod";
        public const string Author = "DDAkebono#0001";
        public const string Company = "BTK-Development";
        public const string Version = "1.1.5";
        public const string DownloadLink = "https://github.com/ddakebono/BTKSAGestureMod/releases";
    }

    public class BTKSAGestureMod : MelonMod
    {
        public static BTKSAGestureMod instance;

        public HarmonyInstance harmony;

        public static string settingsCategory = "BTKSAGestureMod";
        public static string rightHandSetting = "righthand";
        public static string leftHandSetting = "lefthand";
        public static string leftHandActionDisable = "lefthandactiondisable";
        public static string rightHandActionDisable = "righthandactiondisable";

        public override void VRChat_OnUiManagerInit()
        {
            MelonLogger.Msg("BTK Standalone: Gesture Mod - Starting up");

            instance = this;

            if (MelonHandler.Mods.Any(x => x.Info.Name.Equals("BTKCompanionLoader", StringComparison.OrdinalIgnoreCase)))
            {
                MelonLogger.Msg("Hold on a sec! Looks like you've got BTKCompanion installed, this mod is built in and not needed!");
                MelonLogger.Error("BTKSAGestureMod has not started up! (BTKCompanion Running)");
                return;
            }

            MelonPreferences.CreateCategory(settingsCategory, "Gesture Mod");
            MelonPreferences.CreateEntry<bool>(settingsCategory, rightHandSetting, false, "Replace Right Hand Action Menu");
            MelonPreferences.CreateEntry<bool>(settingsCategory, leftHandSetting, false, "Replace Left Hand Action Menu");
            MelonPreferences.CreateEntry<bool>(settingsCategory, rightHandActionDisable, false, "Disable Right Action Menu");
            MelonPreferences.CreateEntry<bool>(settingsCategory, leftHandActionDisable, false, "Disable Left Action Menu");

            //Only initialize for VR users
            if (XRDevice.isPresent)
            {
                int patchCount = 0;

                //Initalize Harmony
                harmony = HarmonyInstance.Create("BTKStandaloneGM");

                //Setup hooks on matching methods in ActionMenuOpener
                foreach(MethodInfo method in typeof(ActionMenuOpener).GetMethods(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (method.Name.Contains("Method_Public_Void_Boolean"))
                    {
                        patchCount++;
                        harmony.Patch(method, new HarmonyMethod(typeof(BTKSAGestureMod).GetMethod("OnActionMenuOpen", BindingFlags.Static | BindingFlags.Public)));
                    }
                }

                MelonLogger.Msg($"Found {patchCount} matching methods in ActionMenuOpener.");
            }
            else
            {
                MelonLogger.Msg("Desktop Mode Detected, Gesture Mod has not started up!");
            }
        }

        /// <summary>
        /// This function will handle the main override of the ActionMenuOpener object according
        /// to the configuration supplied by MelonPrefs
        /// </summary>
        /// <param name="__0">Target state for the ActionMenu, either open or close</param>
        /// <param name="__instance">Instance of ActionMenuOpener __instance.name gives the MenuL or MenuR needed to determine hand</param>
        public static bool OnActionMenuOpen(bool __0, ref ActionMenuOpener __instance)
        {
            //MelonLogger.Log($"ActionMenuOpener OpenActionMenu Called OpenerName: {__instance.name}, BoolState: {__0}");
            if ((MelonPreferences.GetEntryValue<bool>(settingsCategory, rightHandSetting) && __instance.name.Equals("MenuR")) || (MelonPreferences.GetEntryValue<bool>(settingsCategory, leftHandSetting) && __instance.name.Equals("MenuL")))
            {

                if (__0)
                {
                    HandGestureController.Method_Public_Static_Void_Boolean_0(!HandGestureController.Method_Public_Static_Boolean_0());
                }
                else
                {
                    HandGestureController.Method_Public_Static_Void_Boolean_0(__0);
                }
                return false; //Skip original function

            }

            //Check if action menu is disabled for hand
            if ((MelonPreferences.GetEntryValue<bool>(settingsCategory, rightHandActionDisable) && __instance.name.Equals("MenuR")) || (MelonPreferences.GetEntryValue<bool>(settingsCategory, leftHandActionDisable) && __instance.name.Equals("MenuL")))
                return false;

            return true;
        }
    }
}
