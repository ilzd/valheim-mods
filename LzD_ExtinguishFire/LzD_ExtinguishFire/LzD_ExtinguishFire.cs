using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;


namespace LzD_ExtinguishFire
{
    [BepInPlugin(modID, modName, modVersion)]
    [BepInProcess("valheim.exe")]
    public class LzD_ExtinguishFire : BaseUnityPlugin
    {
        public const string modID = "lzd_extinguishfire";
        public const string modName = "LzD Extinguish Fire";
        public const string modVersion = "1.0.0";

        private readonly Harmony harmony = new Harmony(modID);

        private static ConfigEntry<bool> modEnabled;
        private static ConfigEntry<bool> logEnabled;
        private static ConfigEntry<KeyboardShortcut> actionKey;

        void Awake()
        {
            modEnabled = Config.Bind<bool>("1 - Global", "a. Enable mod", true, "Enable or disable the mod completely");
            logEnabled = Config.Bind<bool>("1 - Global", "b. Enable logs", true, "Enable or disable logs completely");
            actionKey = Config.Bind<KeyboardShortcut>("2 - Controls", "a. Collect/Extinguish key", new KeyboardShortcut(KeyCode.LeftAlt), "Defines the key to collect fuel and extinguish fireplaces");

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

        [HarmonyPatch(typeof(Fireplace), nameof(Fireplace.GetHoverText))]
        static class Fireplace_GetHoverText_Patch
        {
            static void Postfix(ref string __result)
            {
                if (!modEnabled.Value) return;

                __result += $"\n[<color=yellow><b>{actionKey.Value.MainKey}</b></color>] Collect fuel / Extinguish fire";
            }
        }

        [HarmonyPatch(typeof(Player), "Update")]
        static class Player_Update_Patch
        {
            static void Postfix(Player __instance)
            {
                if (!modEnabled.Value) return;

                Player player = __instance;
                bool extinguishPressed = UnityInput.Current.GetKeyDown(actionKey.Value.MainKey);

                if (!extinguishPressed) return;

                Fireplace fireplace = player.GetHoverObject()?.GetComponentInParent<Fireplace>();
                if (fireplace == null) return;
                ZNetView zNetView = fireplace.GetComponent<ZNetView>();

                if (zNetView == null) return;

                float fuelTotal = zNetView.GetZDO().GetFloat("fuel");
                if (fuelTotal <= 0f) return;

                int fuelRemovable = (int)Mathf.Floor(fuelTotal);

                if (fuelRemovable == 0)
                {
                    Log("Fire extinguished");
                    zNetView.GetZDO().Set("fuel", 0f);
                    fireplace.m_fuelAddedEffects.Create(fireplace.transform.position, fireplace.transform.rotation);
                    return;
                }

                Log($"{fuelRemovable} fuel units recovered");
                zNetView.GetZDO().Set("fuel", fuelTotal - fuelRemovable);
                GameObject fuelPrefab = ZNetScene.instance.GetPrefab(fireplace.m_fuelItem.name);
                while (fuelRemovable > 0)
                {
                    ItemDrop fuelPack = Instantiate(fuelPrefab, fireplace.transform.position + Vector3.up, Quaternion.identity).GetComponent<ItemDrop>();
                    int packSize = Mathf.Min(fuelPack.m_itemData.m_shared.m_maxStackSize, fuelRemovable);
                    fuelPack.SetStack(packSize);
                    fuelRemovable -= packSize;
                }
            }
        }
    }
}
