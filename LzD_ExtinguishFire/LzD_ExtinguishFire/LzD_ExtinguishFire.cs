using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using System.Collections;

namespace LzD_ExtinguishFire
{
    [BepInPlugin("lzd_extinguishfire", "LzD Extinguish Fire", "0.0.0")]
    [BepInProcess("valheim.exe")]
    public class LzD_ExtinguishFire : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony("lzd.stationscustomranges");

        private static ConfigEntry<bool> modEnabled;

        void Awake()
        {
            modEnabled = Config.Bind<bool>("1 - General", "a. Enabled", true, "Enable or disable the mod completely (restart required)");

            if (!modEnabled.Value) return;

            Debug.Log("Initialized config and debugging");
            harmony.PatchAll();
        }

        void OnDestroy()
        {
            harmony.UnpatchSelf();
        }

        [HarmonyPatch(typeof(Fireplace), nameof(Fireplace.GetHoverText))]
        static class Fireplace_GetHoverText_Patch
        {
            static void Postfix(ref string __result)
            {
                __result += $"\n[<color=yellow><b>LeftAlt</b></color>] Extinguish fire";
            }
        }

        [HarmonyPatch(typeof(Player), "Update")]
        static class Player_Update_Patch
        {
            static void Postfix(Player __instance)
            {
                Player player = __instance;
                bool extractPressed = UnityInput.Current.GetKey(KeyCode.LeftAlt);

                Fireplace fireplace = player.GetHoverObject()?.GetComponentInParent<Fireplace>();
                if (fireplace == null) return;
                ZNetView zNetView = fireplace.GetComponent<ZNetView>();      

                if (zNetView == null || !extractPressed) return;

                float fuelTotal = zNetView.GetZDO().GetFloat("fuel");
                int fuelRemovable = (int) Mathf.Floor(fuelTotal);
                GameObject fuelPrefab = ZNetScene.instance.GetPrefab(fireplace.m_fuelItem.name);
                zNetView.GetZDO().Set("fuel", 0f);

                if(fuelTotal > 0f) fireplace.m_fuelAddedEffects.Create(fireplace.transform.position, fireplace.transform.rotation);

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
