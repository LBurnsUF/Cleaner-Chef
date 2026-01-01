using BepInEx;
using BepInEx.Logging;
using RoR2;

namespace CleanerChef
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    public class CleanerChef : BaseUnityPlugin
    {
        public const string PluginGUID = "rainorshine.CleanChef";
        public const string PluginName = "CleanChef";
        public const string PluginVersion = "1.0.0";

        internal static ManualLogSource Log;

        public void Awake()
        {
            Log = Logger;
            Log.LogInfo("CleanChef: Initializing hardened logic.");

            On.RoR2.Items.ContagiousItemManager.StepInventoryInfection += (orig, inventory, originalItem, limit, isForced) =>
            {
                if (CheckForChefPresence())
                {
                    Log.LogInfo("CleanChef: Chef present, cancelling void.");
                    return false;
                }
                return orig(inventory, originalItem, limit, isForced);
            };
        }

        /// <summary>
        /// Hardened check that scans for the presence of a CraftingController.
        /// Uses FindObjectOfType for accuracy when InstanceTracker is unavailable.
        /// </summary>
        internal static bool CheckForChefPresence()
        {
            return UnityEngine.Object.FindObjectOfType<CraftingController>() != null;
        }
    }
}