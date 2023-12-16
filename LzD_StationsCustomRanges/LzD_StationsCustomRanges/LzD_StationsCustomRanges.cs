using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace LzD_StationsCustomRanges
{
    [BepInPlugin(modID, modName, modVersion)]
    [BepInProcess("valheim.exe")]
    public class LzD_StationsCustomRanges : BaseUnityPlugin
    {
        public const string modID = "lzd_stationscustomranges";
        public const string modName = "LzD Stations Custom Ranges";
        public const string modVersion = "1.0.0";

        private readonly Harmony harmony = new Harmony(modID);

        private static ConfigEntry<bool> modEnabled;
        private static ConfigEntry<bool> logEnabled;

        private static ConfigEntry<bool> rangeEnabled;
        private static ConfigEntry<float> workbenchRange;
        private static ConfigEntry<float> forgeRange;
        private static ConfigEntry<float> stonecutterRange;

        private static ConfigEntry<bool> rangeLvlEnabled;
        private static ConfigEntry<float> workbenchRangeLvl;
        private static ConfigEntry<float> forgeRangeLvl;

        private static ConfigEntry<bool> extMaxDistEnabled;
        private static ConfigEntry<float> extMaxDistance;

        void Awake()
        {
            modEnabled = Config.Bind<bool>("1 - Global", "a. Enable mod", true, "Enable or disable the mod completely");
            logEnabled = Config.Bind<bool>("1 - Global", "b. Enable logs", true, "Enable or disable logs completely");

            rangeEnabled = Config.Bind<bool>("2 - Base Range", "a. Enabled", true, "Enable or disable base range customizations");
            workbenchRange = Config.Bind<float>("2 - Base Range", "b. Workbench", 30f, "Workbench base range");
            forgeRange = Config.Bind<float>("2 - Base Range", "c. Forge", 30f, "Forge base range");
            stonecutterRange = Config.Bind<float>("2 - Base Range", "d. Stonecutter", 54f, "Stonecutter base range");

            rangeLvlEnabled = Config.Bind<bool>("3 - Range per level", "a. Enabled", true, "Enable or disable range per level customizations");
            workbenchRangeLvl = Config.Bind<float>("3 - Range per level", "b. Workbench", 6f, "Workbench range per level");
            forgeRangeLvl = Config.Bind<float>("3 - Range per level", "c. Forge", 4f, "Forge range per level");

            extMaxDistEnabled = Config.Bind<bool>("4 - Station extentions", "a. Enabled", true, "Enable or disable station extention maximum distance customizations");
            extMaxDistance = Config.Bind<float>("4 - Station extentions", "b. MaxDistance", 15f, "Maximum distance for station extentions");

            if (!modEnabled.Value) return;

            harmony.PatchAll();
            Log($"{modName} mod initialized");
        }

        void OnDestroy()
        {
            harmony.UnpatchSelf();
        }

        static void Log(string msg)
        {
            if (!logEnabled.Value) return;
            Debug.Log(msg);
        }

        [HarmonyPatch(typeof(CraftingStation), nameof(CraftingStation.UpdateKnownStationsInRange))]
        static class CraftingStation_UpdateKnownStationsInRange_Patch
        {
            static void Postfix(List<CraftingStation> ___m_allStations)
            {
                foreach (CraftingStation station in ___m_allStations)
                {
                    if (station.m_name == "$piece_workbench")
                    {
                        changeStationRange(station, workbenchRange.Value);
                        changeStationRangePerLevel(station, workbenchRangeLvl.Value);
                        continue;
                    }
                    else if (station.m_name == "$piece_forge")
                    {
                        changeStationRange(station, forgeRange.Value);
                        changeStationRangePerLevel(station, forgeRangeLvl.Value);
                        continue;
                    }
                    else if (station.m_name == "$piece_stonecutter")
                    {
                        changeStationRange(station, stonecutterRange.Value);
                    }
                }

            }

            static void changeStationRange(CraftingStation station, float newRange)
            {
                if (station.m_rangeBuild == newRange || !rangeEnabled.Value) return;

                station.m_rangeBuild = newRange;
                Debug.Log($"Changing {station.m_name} range build to: {newRange}");
            }

            static void changeStationRangePerLevel(CraftingStation station, float newRangePerLevel)
            {
                if (station.m_extraRangePerLevel == newRangePerLevel || !rangeLvlEnabled.Value) return;

                station.m_extraRangePerLevel = newRangePerLevel;
                Debug.Log($"Changing {station.m_name} extra range per level to: {newRangePerLevel}");
            }
        }

        [HarmonyPatch(typeof(StationExtension), "Awake")]
        static class StationExtension_Awake_Patch
        {
            static void Postfix(ref float ___m_maxStationDistance)
            {
                if (___m_maxStationDistance == extMaxDistance.Value || !extMaxDistEnabled.Value) return;

                ___m_maxStationDistance = extMaxDistance.Value;
                Debug.Log($"Changing extension max distance to: {___m_maxStationDistance}");
            }
        }
    }
}