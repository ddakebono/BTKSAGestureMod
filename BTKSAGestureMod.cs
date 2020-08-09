using Harmony;
using MelonLoader;
using System.Reflection;

namespace BTKSAGestureMod
{
    public static class BuildInfo
    {
        public const string Name = "BTKSAGestureMod";
        public const string Author = "DDAkebono#0001";
        public const string Company = "BTK-Development";
        public const string Version = "1.0.0";
        public const string DownloadLink = "https://github.com/ddakebono/BTKSAGestureMod/releases";
    }

    public class BTKSAGestureMod : MelonMod
    {
        public static BTKSAGestureMod instance;

        public HarmonyInstance harmony;

        public static string settingsCategory = "BTKSAGestureMod";
        public static string rightHandSetting = "righthand";
        public static string leftHandSetting = "lefthand";

        public override void VRChat_OnUiManagerInit()
        {
            MelonLogger.Log("BTK Standalone: Gesture Mod - Starting up");

            instance = this;

            MelonPrefs.RegisterCategory(settingsCategory, "Gesture Mod");
            MelonPrefs.RegisterBool(settingsCategory, rightHandSetting, false, "Replace Right Hand Action Menu");
            MelonPrefs.RegisterBool(settingsCategory, leftHandSetting, false, "Replace Left Hand Action Menu");


            //Initalize Harmony
            harmony = HarmonyInstance.Create("BTKStandalone");
            //OpenActionMenu - Takes bool for open or close?
            harmony.Patch(typeof(ActionMenuOpener).GetMethod("Method_Public_Void_Boolean_2", BindingFlags.Public | BindingFlags.Instance), new HarmonyMethod(typeof(BTKSAGestureMod).GetMethod("OnActionMenuOpen", BindingFlags.Static | BindingFlags.Public)));


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

            return true;
        }
    }
}
