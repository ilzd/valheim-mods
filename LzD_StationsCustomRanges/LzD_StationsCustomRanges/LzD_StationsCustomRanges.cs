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
        public const string modVersion = "1.1.13";

        private readonly Harmony harmony = new Harmony(modID);

        //general settings
        private static ConfigEntry<bool> modEnabled;
        private static ConfigEntry<bool> logsEnabled;
        private static ConfigEntry<bool> rangeEnabled;
        private static ConfigEntry<bool> rangeLvlEnabled;
        private static ConfigEntry<bool> extMaxDistEnabled;

        //global settings
        private static ConfigEntry<bool> globalEnabled;
        private static ConfigEntry<float> globalRange;
        private static ConfigEntry<float> globalRangePerLevel;

        //workbench settings
        private static ConfigEntry<bool> workbenchEnabled;
        private static ConfigEntry<float> workbenchRange;
        private static ConfigEntry<float> workbenchRangeLvl;

        //forge settingsR
        private static ConfigEntry<bool> forgeEnabled;
        private static ConfigEntry<float> forgeRange;
        private static ConfigEntry<float> forgeRangeLvl;

        //stonecutter settings
        private static ConfigEntry<bool> stonecutterEnabled;
        private static ConfigEntry<float> stonecutterRange;

        //blackforge settings
        private static ConfigEntry<bool> blackforgeEnabled;
        private static ConfigEntry<float> blackforgeRange;
        private static ConfigEntry<float> blackforgeRangeLvl;

        //artisantable settings
        private static ConfigEntry<bool> artisanEnabled;
        private static ConfigEntry<float> artisanTableRange;

        //galdrtable settings
        private static ConfigEntry<bool> galdrEnabled;
        private static ConfigEntry<float> galdrTableRange;
        private static ConfigEntry<float> galdrTableRangeLvl;

        //extension settings
        private static ConfigEntry<float> extMaxDist;

        void Awake()
        {
            modEnabled = Config.Bind<bool>("1 - General", "a. Mod enabled", true, "Enable or disable the mod completely");
            logsEnabled = Config.Bind<bool>("1 - General", "b. Logs enabled", false, "Enable or disable logs completely");
            rangeEnabled = Config.Bind<bool>("1 - General", "c. Base range mods enabled", true, "Enable base or disable range customizations completely");
            rangeLvlEnabled = Config.Bind<bool>("1 - General", "d. Range per level mods enabled", true, "Enable or disable range per level customizations completely");

            globalEnabled = Config.Bind<bool>("2 - Global", "a. Global settings enabled", false, "Enable or disable settings that apply to all stations");
            globalRange = Config.Bind<float>("2 - Global", "b. Global base range", 30f, "Base range to apply to all stations");
            globalRangePerLevel = Config.Bind<float>("2 - Global", "c. Global range per level", 6f, "Range per level to apply to all stations");

            workbenchEnabled = Config.Bind<bool>("3 - Workbench", "a. Enabled", true, "Enable or disable workbench modifications completely");
            workbenchRange = Config.Bind<float>("3 - Workbench", "b. Base range", 30f, "Workbench base range");
            workbenchRangeLvl = Config.Bind<float>("3 - Workbench", "c. Range per level", 6f, "Workbench range per level");

            forgeEnabled = Config.Bind<bool>("4 - Forge", "a. Enabled", true, "Enable or disable forge modifications completely");
            forgeRange = Config.Bind<float>("4 - Forge", "b. Base range", 30f, "Forge base range");
            forgeRangeLvl = Config.Bind<float>("4 - Forge", "c. Range per level", 4f, "Forge range per level");

            stonecutterEnabled = Config.Bind<bool>("5 - Stonecutter", "a. Enabled", true, "Enable or disable stonecutter modifications completely");
            stonecutterRange = Config.Bind<float>("5 - Stonecutter", "b. Base range", 54f, "Stonecutter base range");

            blackforgeEnabled = Config.Bind<bool>("6 - Blackforge", "a. Enabled", true, "Enable or disable blackforge modifications completely");
            blackforgeRange = Config.Bind<float>("6 - Blackforge", "b. Base range", 40f, "Blackforge base range");
            blackforgeRangeLvl = Config.Bind<float>("6 - Blackforge", "c. Range per level", 7f, "Blackforge range per level");

            artisanEnabled = Config.Bind<bool>("7 - Artisan Table", "a. Enabled", true, "Enable or disable artisan table modifications completely");
            artisanTableRange = Config.Bind<float>("7 - Artisan Table", "b. Base range", 54f, "Artisan table base range");

            galdrEnabled = Config.Bind<bool>("8 - Galdr Table", "a. Enabled", true, "Enable or disable galdr table modifications completely");
            galdrTableRange = Config.Bind<float>("8 - Galdr Table", "b. Base range", 40f, "Galdr table base range");
            galdrTableRangeLvl = Config.Bind<float>("8 - Galdr Table", "c. Range per level", 7f, "Galdr table range per level");

            extMaxDistEnabled = Config.Bind<bool>("9 - Upgrades build distance", "a. Enabled", true, "Enable or disable station upgrades maximum distance customizations completely");
            extMaxDist = Config.Bind<float>("9 - Upgrades build distance", "b. Max Distance", 15f, "Maximum distance multiplier for station upgrades");

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
            if (!logsEnabled.Value) return;
            Debug.Log(msg);
        }

        [HarmonyPatch(typeof(CraftingStation), nameof(CraftingStation.UpdateKnownStationsInRange))]
        static class CraftingStation_UpdateKnownStationsInRange_Patch
        {
            static void Postfix(List<CraftingStation> ___m_allStations)
            {
                if (!modEnabled.Value) return;

                foreach (CraftingStation station in ___m_allStations)
                {
                    if (station.m_name == "$piece_workbench")
                    {
                        if (!workbenchEnabled.Value) continue;
                        changeStationRange(station, workbenchRange.Value);
                        changeStationRangePerLevel(station, workbenchRangeLvl.Value);
                        continue;
                    }
                    else if (station.m_name == "$piece_forge")
                    {
                        if (!forgeEnabled.Value) continue;
                        changeStationRange(station, forgeRange.Value);
                        changeStationRangePerLevel(station, forgeRangeLvl.Value);
                        continue;
                    }
                    else if (station.m_name == "$piece_stonecutter")
                    {
                        if (!stonecutterEnabled.Value) continue;
                        changeStationRange(station, stonecutterRange.Value);
                    }
                    else if (station.m_name == "$piece_blackforge")
                    {
                        if (!blackforgeEnabled.Value) continue;
                        changeStationRange(station, blackforgeRange.Value);
                        changeStationRangePerLevel(station, blackforgeRangeLvl.Value);
                    }
                    else if (station.m_name == "$piece_artisanstation")
                    {
                        if (!artisanEnabled.Value) continue;
                        changeStationRange(station, artisanTableRange.Value);
                    }
                    else if (station.m_name == "$piece_magetable")
                    {
                        if (!galdrEnabled.Value) continue;
                        changeStationRange(station, galdrTableRange.Value);
                        changeStationRangePerLevel(station, galdrTableRangeLvl.Value);
                    }
                }

            }

            static void changeStationRange(CraftingStation station, float newRange)
            {
                if (globalEnabled.Value) newRange = globalRange.Value;
                if (station.m_rangeBuild == newRange || !rangeEnabled.Value) return;

                station.m_rangeBuild = newRange;
                Log($"Changing {station.m_name} range build to: {newRange}");
            }

            static void changeStationRangePerLevel(CraftingStation station, float newRangePerLevel)
            {
                if (globalEnabled.Value) newRangePerLevel = globalRangePerLevel.Value;
                if (station.m_extraRangePerLevel == newRangePerLevel || !rangeLvlEnabled.Value) return;

                station.m_extraRangePerLevel = newRangePerLevel;
                Log($"Changing {station.m_name} extra range per level to: {newRangePerLevel}");
            }
        }

        [HarmonyPatch(typeof(StationExtension), "Awake")]
        static class StationExtension_Awake_Patch
        {
            static void Postfix(ref float ___m_maxStationDistance)
            {
                if (___m_maxStationDistance == extMaxDist.Value || !extMaxDistEnabled.Value || !modEnabled.Value) return;

                ___m_maxStationDistance = extMaxDist.Value;
                Log($"Changing extension max distance to: {___m_maxStationDistance}");
            }
        }
    }
}