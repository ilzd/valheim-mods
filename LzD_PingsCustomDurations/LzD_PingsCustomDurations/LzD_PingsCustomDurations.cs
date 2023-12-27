﻿using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using static Chat;

namespace LzD_StationsCustomRanges
{
    [BepInPlugin(modID, modName, modVersion)]
    [BepInProcess("valheim.exe")]
    public class LzD_PingsCustomDurations : BaseUnityPlugin
    {
        public const string modID = "lzd_pingscustomdurations";
        public const string modName = "LzD Pings Custom Durations";
        public const string modVersion = "1.0.0";

        private readonly Harmony harmony = new Harmony(modID);

        //general settings
        private static ConfigEntry<bool> modEnabled;
        private static ConfigEntry<bool> logsEnabled;

        private static ConfigEntry<float> pingsDuration;
        private static ConfigEntry<KeyboardShortcut> freezePingKey;
        private static ConfigEntry<KeyboardShortcut> clearPingKey;

        void Awake()
        {
            modEnabled = Config.Bind<bool>("1 - General", "a. Mod enabled", true, "Enable or disable the mod completely");
            logsEnabled = Config.Bind<bool>("1 - General", "b. Logs enabled", false, "Enable or disable logs completely");

            pingsDuration = Config.Bind<float>("2 - Pings and Shouts", "a. Duration", 15.0f, "Enable or disable logs completely");
            freezePingKey = Config.Bind<KeyboardShortcut>("2 - Pings and Shouts", "b. Freeze pings and shouts hotkey", new KeyboardShortcut(KeyCode.RightAlt), "Defines the key to freeze pings and shouts");
            clearPingKey = Config.Bind<KeyboardShortcut>("2 - Pings and Shouts", "b. Clear pings and shouts hotkey", new KeyboardShortcut(KeyCode.RightControl), "Defines the key to clear pings and shouts");

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

        [HarmonyPatch(typeof(Chat), "AddInworldText")]
        static class Chat_Awake_Patch
        {
            static void Postfix(ref float ___m_worldTextTTL)
            {
                if (___m_worldTextTTL == pingsDuration.Value) return;
                Log($"Changing worldTextTTl to {pingsDuration.Value}");
                ___m_worldTextTTL = pingsDuration.Value;
            }
        }

        [HarmonyPatch(typeof(Chat), "UpdateWorldTexts")]
        static class Chat_UpdateWorldTexts_Patch
        {
            static void Postfix(float ___m_worldTextTTL, List<WorldTextInstance> ___m_worldTexts, float dt)
            {
                updateFrozenWordTextPositions(___m_worldTexts, dt);

                bool clearPingsPressed = UnityInput.Current.GetKeyDown(clearPingKey.Value.MainKey);
                if (clearPingsPressed)
                {
                    clearWorldTexts(___m_worldTexts, ___m_worldTextTTL);
                    return;
                }

                bool freezePingsPressed = UnityInput.Current.GetKeyDown(freezePingKey.Value.MainKey);
                if (freezePingsPressed) freezeWorldTexts(___m_worldTexts);
            }

            static void updateFrozenWordTextPositions(List<WorldTextInstance> worldTexts, float dt)
            {
                foreach (WorldTextInstance worldText in worldTexts)
                {
                    if (worldText.m_timer < 0) worldText.m_position.y -= dt * 0.15f;
                }
            }


            static void freezeWorldTexts(List<WorldTextInstance> worldTexts)
            {
                foreach (WorldTextInstance worldText in worldTexts)
                {
                    worldText.m_timer = -86.400f;
                }
                Log("Freezing pings and shouts.");
            }

            static void clearWorldTexts(List<WorldTextInstance> worldTexts, float maxDuration)
            {
                foreach (WorldTextInstance worldText in worldTexts)
                {
                    worldText.m_timer = maxDuration;
                }
                Log("Clearing pings and shouts.");
            }
        }
    }
}