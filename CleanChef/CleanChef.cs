using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using RiskOfOptions;
using RiskOfOptions.Options;
using RoR2;
using UnityEngine;

namespace CleanerChef
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency("com.rune580.riskofoptions")]

    public class CleanerChef : BaseUnityPlugin
    {
        public const string PluginGUID = "rainorshine.CleanChef";
        public const string PluginName = "CleanChef";
        public const string PluginVersion = "1.0.0";
        public static ConfigEntry<bool> HaltCorruption;

        internal static ManualLogSource Log;

        public void Awake()
        {
            Log = Logger;

            HaltCorruption = Config.Bind(
                "General",
                "Halt Void Corruption",
                true,
                "If enabled, void items will not corrupt base items while a Chef Crafting station is present in the scene."
            );

            ModSettingsManager.AddOption(new CheckBoxOption(HaltCorruption));

            On.RoR2.Items.ContagiousItemManager.StepInventoryInfection += (orig, inventory, originalItem, limit, isForced) =>
            {
                if (CheckForChefPresence())
                {
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
            var vanillaController = UnityEngine.Object.FindObjectOfType<CraftingController>();
            if (vanillaController != null) return true;

            foreach (var go in UnityEngine.Object.FindObjectsOfType<GameObject>())
            {
                if (go.name.Contains("CraftingController") || go.name.Contains("ChefStation"))
                {
                    return true;
                }
            }

            return false;
        }
    }
}