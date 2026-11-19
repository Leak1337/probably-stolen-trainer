using System;
using UnityEngine;

namespace ProbablyStolenTrainer
{
    public sealed partial class TrainerPanel
    {
        private void DrawMaintenance(PlayerStore store)
        {
            GUILayout.Label(Text("Gesundheit", "Health"));
            HealthData health = store.healthData;
            if (health != null)
            {
                bool wounded = health.isWoundedFresh || health.woundState > 0;
                GUILayout.Label(wounded
                    ? Text($"Verletzungswert · {Number(health.woundState)}", $"Wound level · {Number(health.woundState)}")
                    : Text("Keine Verletzung", "No wounds"));
                if (EnabledButton(Text("Verletzungen behandeln", "Treat wounds"), wounded, 225))
                {
                    TreatWounds(health);
                    MapUIManager map = GetDumpingGroundsMap();
                    if (map != null)
                        map.UpdateScavUI();

                    message = Text("Verletzungen behandelt.", "Wounds treated.");
                }
            }

            GUILayout.Space(16);
            GUILayout.Label(Text("Eigenes Ladeninventar", "Owned shop inventory"));
            EmporiumEntry emporium = EmporiumEntry.Instance;
            GameGridInventory inventory = emporium != null ? emporium.invElement : null;
            if (inventory == null)
            {
                GUILayout.Label(Text("Ladeninventar nicht verfügbar.", "Shop inventory is unavailable."));
                return;
            }

            if (GUILayout.Button(Text("Haltbarkeit wiederherstellen", "Restore durability"), GUILayout.Width(225)))
            {
                string count = Number(MaintainStock(inventory, RepairStockItem));
                message = Text($"Reparierte Inventareinträge · {count}", $"Inventory entries repaired · {count}");
            }

            if (GUILayout.Button(Text("Energiequellen aufladen", "Recharge power sources"), GUILayout.Width(225)))
            {
                string count = Number(MaintainStock(inventory, RechargeStockItem));
                message = Text($"Aufgeladene Inventareinträge · {count}", $"Inventory entries recharged · {count}");
            }

            GUILayout.Label(Text("Energiequellen müssen wiederaufladbar sein.", "Power sources must be rechargeable."));
        }

        private static void TreatWounds(HealthData health)
        {
            health.OnHealingItemUsed();
            health.woundState = 0;
            health.isWoundStable = true;
        }

        private static int MaintainStock(GameGridInventory inventory, Func<GameItem, bool> maintain)
        {
            int changed = 0;
            foreach (GameItem item in GraphUtils.FindAllChildrenType<GameItem>(inventory, GeneralHelper.IsItemOwned))
            {
                if (maintain(item))
                    changed++;
            }

            return changed;
        }

        private static bool RepairStockItem(GameItem item)
        {
            if (!item.IsTag(DurabilityHelper.DURABILITY_TAG))
                return false;

            int maximum = DurabilityHelper.GetMaxDurability(item);
            if (maximum <= 0 || DurabilityHelper.GetRealDurability(item) >= maximum)
                return false;

            DurabilityHelper.SetCurrentDurability(item, maximum);
            return true;
        }

        private static bool RechargeStockItem(GameItem item)
        {
            if (!item.IsTag(PowerHelper.POWER_SOURCE_ITEM_TAG) || PowerHelper.IsDisposablePowerSource(item))
                return false;

            int maximum = item.GetTagReadonly(PowerHelper.POWER_SOURCE_ITEM_MAX_ENERGY).GetInt();
            if (maximum <= 0 || PowerHelper.IsPowerSourceFull(item))
                return false;

            PowerHelper.SetPowerSourceAt(item, maximum);
            return true;
        }
    }
}
