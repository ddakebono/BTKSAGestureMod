using Harmony;
using MelonLoader;
using System.IO;
using System.Reflection;
using UnityEngine.XR;

namespace BTKSAGestureMod
{
    public static class BuildInfo
    {
        public const string Name = "BTKSAGestureMod";
        public const string Author = "DDAkebono#0001";
        public const string Company = "BTK-Development";
        public const string Version = "1.1.2";
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
            MelonLogger.Log("BTK Standalone: Gesture Mod - Starting up");

            instance = this;

            if (Directory.Exists("BTKCompanion"))
            {
                MelonLogger.Log("Woah, hold on a sec, it seems you might be running BTKCompanion, if this is true GestureMod is built into that, and you should not be using this!");
                MelonLogger.Log("If you are not currently using BTKCompanion please remove the BTKCompanion folder from your VRChat installation!");
                MelonLogger.LogError("Gesture Mod has not started up! (BTKCompanion Exists)");
                return;
            }

            MelonPrefs.RegisterCategory(settingsCategory, "Gesture Mod");
            MelonPrefs.RegisterBool(settingsCategory, rightHandSetting, false, "Replace Right Hand Action Menu");
            MelonPrefs.RegisterBool(settingsCategory, leftHandSetting, false, "Replace Left Hand Action Menu");
            MelonPrefs.RegisterBool(settingsCategory, rightHandActionDisable, false, "Disable Right Action Menu");
            MelonPrefs.RegisterBool(settingsCategory, leftHandActionDisable, false, "Disable Left Action Menu");

            //Only initialize for VR users
            if (XRDevice.isPresent)
            {
                //Initalize Harmony
                harmony = HarmonyInstance.Create("BTKStandaloneGM");

                //Setup hooks on matching methods in ActionMenuOpener
                foreach(MethodInfo method in typeof(ActionMenuOpener).GetMethods(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (method.Name.Contains("Method_Public_Void_Boolean"))
                    {
                        //MelonLogger.Log($"Found method to patch {method.Name}");
                        harmony.Patch(method, new HarmonyMethod(typeof(BTKSAGestureMod).GetMethod("OnActionMenuOpen", BindingFlags.Static | BindingFlags.Public)));
                    }
                }
            }
            else
            {
                MelonLogger.Log("Desktop Mode Detected, Gesture Mod has not started up!");
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
            if ((MelonPrefs.GetBool(settingsCategory, rightHandSetting) && __instance.name.Equals("MenuR")) || (MelonPrefs.GetBool(settingsCategory, leftHandSetting) && __instance.name.Equals("MenuL")))
            {
                if (__0)
                {
                    HandGestureController.Method_Public_Static_Void_Boolean_0(!HandGestureController.field_Private_Static_Boolean_0);
                }
                else
                {
                    HandGestureController.Method_Public_Static_Void_Boolean_0(__0);
                }
                return false; //Skip original function

            }

            //Check if action menu is disabled for hand
            if ((MelonPrefs.GetBool(settingsCategory, rightHandActionDisable) && __instance.name.Equals("MenuR")) || (MelonPrefs.GetBool(settingsCategory, leftHandActionDisable) && __instance.name.Equals("MenuL")))
                return false;

            return true;
        }
    }
}
