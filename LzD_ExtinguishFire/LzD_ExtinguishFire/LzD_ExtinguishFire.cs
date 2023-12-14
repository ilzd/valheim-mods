using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace LzD_ExtinguishFire
{
    [BepInPlugin("lzd_extinguishfire", "LzD Extinguish Fire", "0.0.0")]
    [BepInProcess("valheim.exe")]
    public class LzD_ExtinguishFire : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony("lzd_extinguishfire");

        private static ConfigEntry<bool> modEnabled;
        private static ConfigEntry<bool> logEnabled;
        private static ConfigEntry<KeyboardShortcut> extinguishKey;

        void Awake()
        {
            modEnabled = Config.Bind<bool>("1 - Global", "a. Enable mod", true, "Enable or disable the mod completely");
            logEnabled = Config.Bind<bool>("1 - Global", "b. Enable logs", true, "Enable or disable logs completely");
            extinguishKey = Config.Bind<KeyboardShortcut>("2 - Controls", "a. Extinguish Key", new KeyboardShortcut(KeyCode.LeftAlt), "Define the key that extinguishes fireplaces");

            if (!modEnabled.Value) return;

            harmony.PatchAll();
            Log("lzd_extinguishfire mod initialized");
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

                __result += $"\n[<color=yellow><b>{extinguishKey.Value.MainKey}</b></color>] Extinguish fire";
            }
        }

        [HarmonyPatch(typeof(Player), "Update")]
        static class Player_Update_Patch
        {
            static void Postfix(Player __instance)
            {
                if (!modEnabled.Value) return;

                Player player = __instance;
                bool extinguishPressed = UnityInput.Current.GetKeyDown(extinguishKey.Value.MainKey);

                Fireplace fireplace = player.GetHoverObject()?.GetComponentInParent<Fireplace>();
                if (fireplace == null) return;
                ZNetView zNetView = fireplace.GetComponent<ZNetView>();      

                if (zNetView == null || !extinguishPressed) return;

                float fuelTotal = zNetView.GetZDO().GetFloat("fuel");
                int fuelRemovable = (int) Mathf.Floor(fuelTotal);
                GameObject fuelPrefab = ZNetScene.instance.GetPrefab(fireplace.m_fuelItem.name);
                zNetView.GetZDO().Set("fuel", 0f);

                if (fuelTotal > 0f)
                {
                    Log("Fire extinguished");
                    fireplace.m_fuelAddedEffects.Create(fireplace.transform.position, fireplace.transform.rotation);
                }

                Log($"{fuelRemovable} fuel units recovered");
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
