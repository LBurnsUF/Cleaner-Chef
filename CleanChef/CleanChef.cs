using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using RiskOfOptions;
using RiskOfOptions.Options;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CleanerChef
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency("com.rune580.riskofoptions")]
    public class CleanerChef : BaseUnityPlugin
    {
        public const string PluginGUID = "rainorshine.CleanChef";
        public const string PluginName = "CleanChef";
        public const string PluginVersion = "1.1.0";

        public static ConfigEntry<bool> HaltCorruption;
        private static Dictionary<string, ConfigEntry<bool>> _stageBlacklistConfigs = new Dictionary<string, ConfigEntry<bool>>();

        private static bool _chefFoundInCurrentStage = false;

        internal static ManualLogSource Log;

        public void Awake()
        {
            Log = Logger;

            HaltCorruption = Config.Bind("General", "Halt Void Corruption", true, "Master toggle for the mod logic.");
            ModSettingsManager.AddOption(new CheckBoxOption(HaltCorruption));

            CleanerChefAPI.RaiseHaltCorruptionChanged(HaltCorruption.Value);
            HaltCorruption.SettingChanged += OnHaltCorruptionChanged;
            Stage.onStageStartGlobal += OnStageStart;
            RoR2Application.onLoad += SetupStageConfig;

            On.RoR2.Items.ContagiousItemManager.StepInventoryInfection += (orig, inventory, originalItem, limit, isForced) =>
            {

                if (HaltCorruption.Value && !IsCurrentStageBlacklisted() && CheckForChefPresenceCached())
                {
                    return false;
                }

                return orig(inventory, originalItem, limit, isForced);
            };
        }

        public void OnDestroy()
        {
            HaltCorruption.SettingChanged -= OnHaltCorruptionChanged;
            RoR2Application.onLoad -= SetupStageConfig;
        }

        private void OnStageStart(Stage stage)
        {
            _chefFoundInCurrentStage = false;
            Stage.onStageStartGlobal -= OnStageStart;
        }

        private void OnHaltCorruptionChanged(object sender, System.EventArgs e)
        {
            CleanerChefAPI.RaiseHaltCorruptionChanged(HaltCorruption.Value);
        }

        private void SetupStageConfig()
        {
            string[] defaultBlacklist = { "moon2", "meridian", "voidstage", "limbo", "mysteryspace" };

            var uniqueStages = SceneCatalog.allSceneDefs
                .Where(s => s.sceneType == SceneType.Stage)
                .GroupBy(s => s.baseSceneName)
                .Select(g => g.First())
                .OrderBy(s => s.baseSceneName);

            foreach (var scene in uniqueStages)
            {
                string rawName = scene.baseSceneName;
                if (_stageBlacklistConfigs.ContainsKey(rawName)) continue;

                bool isDefaultBlacklisted = defaultBlacklist.Contains(rawName.ToLower());
                string friendlyName = Language.GetString(scene.nameToken);

                var config = Config.Bind(
                    "Stage Blacklist",
                    rawName,
                    isDefaultBlacklisted,
                    new ConfigDescription($"Disable item protection on {friendlyName}?", null)
                );

                _stageBlacklistConfigs[rawName] = config;
                ModSettingsManager.AddOption(new CheckBoxOption(config));
            }
        }

        private bool IsCurrentStageBlacklisted()
        {
            var scene = SceneCatalog.GetSceneDefForCurrentScene();
            if (scene != null && _stageBlacklistConfigs.TryGetValue(scene.baseSceneName, out var config))
            {
                return config.Value;
            }
            return false;
        }

        internal static bool CheckForChefPresenceCached()
        {
            if (_chefFoundInCurrentStage) return true;

            _chefFoundInCurrentStage = PerformChefPresenceCheck();
            return _chefFoundInCurrentStage;
        }

        private static bool PerformChefPresenceCheck()
        {
            if (UnityEngine.Object.FindObjectOfType<CraftingController>() != null) return true;

            var allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
            for (int i = 0; i < allObjects.Length; i++)
            {
                var name = allObjects[i].name;
                if (name.Contains("CraftingController") || name.Contains("ChefStation"))
                {
                    return true;
                }
            }
            return false;
        }
    }
}