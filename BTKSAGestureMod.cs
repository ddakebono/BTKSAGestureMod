using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine.XR;

namespace BTKSAGestureMod
{
    public static class BuildInfo
    {
        public const string Name = "BTKSAGestureMod";
        public const string Author = "DDAkebono#0001";
        public const string Company = "BTK-Development";
        public const string Version = "1.1.8";
        public const string DownloadLink = "https://github.com/ddakebono/BTKSAGestureMod/releases";
    }

    public class BTKSAGestureMod : MelonMod
    {
        public static BTKSAGestureMod Instance;

        public MethodInfo HandGestureCtrlMethod;
        public bool IsGestureModReady;

        public static string settingsCategory = "BTKSAGestureMod";
        public static string rightHandSetting = "righthand";
        public static string leftHandSetting = "lefthand";
        public static string leftHandActionDisable = "lefthandactiondisable";
        public static string rightHandActionDisable = "righthandactiondisable";

        int scenesLoaded = 0;

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (scenesLoaded > 2) return;
            scenesLoaded++;
            if (scenesLoaded == 2)
                UiManagerInit();
        }

        private void UiManagerInit()
        {
            MelonLogger.Msg("BTK Standalone: Gesture Mod - Starting up");

            Instance = this;

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
            if (true)//XRDevice.isPresent)
            {
                HandGestureCtrlMethod = typeof(HandGestureController).GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(x => x.Name.Contains("Method_Public_Static_Void_Boolean_"));
                if (HandGestureCtrlMethod == null)
                    MelonLogger.Error("Could not find required method in HandGestureController! GestureMod will not function.");

                IsGestureModReady = true;
            }
            else
            {
                MelonLogger.Msg("Desktop Mode Detected, Gesture Mod has not started up!");
            }
        }
    }
    
    [HarmonyPatch]
    class ActionMenuPatches 
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            return typeof(ActionMenuOpener).GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(x => x.Name.Contains("Method_Public_Void_Boolean")).Cast<MethodBase>();
        }

        /// <summary>
        /// This function will handle the main override of the ActionMenuOpener object according
        /// to the configuration supplied by MelonPrefs
        /// </summary>
        /// <param name="__0">Target state for the ActionMenu, either open or close</param>
        /// <param name="__instance">Instance of ActionMenuOpener __instance.name gives the MenuL or MenuR needed to determine hand</param>
        static bool Prefix(bool __0, ref ActionMenuOpener __instance)
        {
            if (!BTKSAGestureMod.Instance.IsGestureModReady) return true;
            
            if ((MelonPreferences.GetEntryValue<bool>(BTKSAGestureMod.settingsCategory, BTKSAGestureMod.rightHandSetting) &&
                 __instance.name.Equals("MenuR")) ||
                (MelonPreferences.GetEntryValue<bool>(BTKSAGestureMod.settingsCategory, BTKSAGestureMod.leftHandSetting) &&
                 __instance.name.Equals("MenuL")) && BTKSAGestureMod.Instance.HandGestureCtrlMethod != null)
            {

                if (__0)
                {
                    BTKSAGestureMod.Instance.HandGestureCtrlMethod.Invoke(BTKSAGestureMod.Instance,
                        new Object[] {!HandGestureController.Method_Public_Static_Boolean_PDM_0()});
                }
                else
                {
                    BTKSAGestureMod.Instance.HandGestureCtrlMethod.Invoke(BTKSAGestureMod.Instance,
                        new Object[] {__0});
                }

                return false; //Skip original function

            }

            //Check if action menu is disabled for hand
            if ((MelonPreferences.GetEntryValue<bool>(BTKSAGestureMod.settingsCategory, BTKSAGestureMod.rightHandActionDisable) &&
                 __instance.name.Equals("MenuR")) ||
                (MelonPreferences.GetEntryValue<bool>(BTKSAGestureMod.settingsCategory, BTKSAGestureMod.leftHandActionDisable) &&
                 __instance.name.Equals("MenuL")))
                return false;

            return true;
        }
    }
}
