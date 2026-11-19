using System;
using System.Collections.Generic;
using UnityEngine;
using static ProbablyStolenTrainer.TrainerPanel;

namespace ProbablyStolenTrainer
{
    internal sealed class InspectionOptions
    {
        private readonly ConfigEntry<bool> wantedCustomers;
        private readonly ConfigEntry<bool> augmentedCustomers;
        private readonly ConfigEntry<bool> authenticity;
        private readonly ConfigEntry<bool> quality;

        internal InspectionOptions(ModConfig config)
        {
            wantedCustomers = Define(config, "show_wanted_customers", "Gesuchte Personen erkennen", "Identify wanted customers");
            augmentedCustomers = Define(config, "show_augmented_customers", "Augmentierte Kunden erkennen", "Identify augmented customers");
            authenticity = Define(config, "show_authenticity", "Fälschungen, Diebesgut und Schmuggelware erkennen", "Identify fakes, stolen goods, and contraband");
            quality = Define(config, "show_quality", "Wahre Qualität und Gutachterwert anzeigen", "Show true quality and appraisal value");
        }

        private static ConfigEntry<bool> Define(ModConfig config, string key, string de, string en) =>
            config.Define(key, false, "Inspection", () => Text(de, en));

        internal void DrawOptions()
        {
            GUILayout.Label(Text("Aktueller Kunde", "Current customer"));
            DrawToggle(wantedCustomers);
            DrawToggle(augmentedCustomers);
            GUILayout.Space(12);
            GUILayout.Label(Text("Im Infofenster der Ware", "In item tooltips"));
            DrawToggle(authenticity);
            DrawToggle(quality);
        }

        private static void DrawToggle(ConfigEntry<bool> option)
        {
            bool next = GUILayout.Toggle(option.Value, option.Label());
            if (next != option.Value)
                option.Set(next);
        }

        internal void DrawCustomer(PlayerStore store)
        {
            if (Event.current.type != EventType.Repaint || (!wantedCustomers.Value && !augmentedCustomers.Value))
                return;

            StoreClient client = store.currentClientInstance?.GetClientBlueprint();
            if (client == null)
                return;

            var lines = new List<string> { Text("Kunde", "Customer") };
            if (!string.IsNullOrWhiteSpace(client.displayName))
                lines[0] = client.displayName;

            if (wantedCustomers.Value)
                lines.Add(client.isWanted ? Text("Gesucht", "Wanted") : Text("Kein Fahndungseintrag", "No warrant"));

            if (augmentedCustomers.Value)
            {
                lines.Add(client.isAug
                    ? Text($"Augmentiert · Stufe {Number(client.augTier)}", $"Augmented · Tier {Number(client.augTier)}")
                    : Text("Keine Augmentierung", "No augmentation"));
            }

            var bounds = new Rect(Math.Max(4, Screen.width - 280), 38, 264, 14 + lines.Count * 17);
            GUI.Box(bounds, string.Join("\n", lines));
        }

        internal void AppendItemDetails(RichTextBuilder builder, GameItem item)
        {
            if (item == null || (!authenticity.Value && !quality.Value))
                return;

            var details = new List<string>();
            if (authenticity.Value)
                AddAuthenticity(details, item);

            if (quality.Value)
                AddQuality(details, item);

            if (details.Count == 0)
                return;

            builder.AddLine(Text("Trainer-Analyse", "Trainer inspection"), bold: true);
            foreach (string detail in details)
                builder.AddLine(detail);
        }

        private static void AddAuthenticity(List<string> details, GameItem item)
        {
            bool counterfeit = item.IsTag(WineHelper.WINE_COUNTERFEIT_TAG) || RibwichHelper.IsCounterfeit(item);
            bool genuine = false;
            bool replica = false;
            bool durabilityMisrepresented = false;
            if (item.itemFeatures != null)
            {
                foreach (ItemFeature feature in item.itemFeatures)
                {
                    if (feature?.identifier == DurabilityHelper.DURABILITY_TAG && !feature.IsFeatureMatch())
                        durabilityMisrepresented = true;

                    string id = feature?.realCondition?.identifier;
                    switch (id)
                    {
                        case "counterfeitCigarette":
                        case "counterfeitStamp":
                        case "counterfeitInjector":
                            counterfeit = true;
                            break;
                        case "genuineCigarette":
                        case "genuineStamp":
                        case "genuineInjector":
                        case "expiredInjector":
                            genuine = true;
                            break;
                        case "replica":
                            replica = true;
                            break;
                    }
                }
            }

            if (counterfeit)
                details.Add(Text("Echtheit · Fälschung", "Authenticity · Counterfeit"));
            else if (replica)
                details.Add(Text("Echtheit · Replik", "Authenticity · Replica"));
            else if (genuine)
                details.Add(Text("Echtheit · Echt", "Authenticity · Genuine"));

            if (StolenHelper.IsStolenItem(item))
                details.Add(Text("Diebesgut", "Stolen goods"));
            if (ContrabandHelper.IsContraband(item))
                details.Add(Text("Schmuggelware", "Contraband"));

            if (durabilityMisrepresented)
                details.Add(Text("Haltbarkeitsangabe verfälscht", "Misrepresented durability"));
        }

        private static void AddQuality(List<string> details, GameItem item)
        {
            if (item.itemFeatures != null)
            {
                foreach (ItemFeature feature in item.itemFeatures)
                {
                    ItemCondition condition = feature?.realCondition;
                    if (condition == null || string.IsNullOrWhiteSpace(condition.display))
                        continue;

                    string label = QualityLabel(condition.category, condition.identifier);
                    if (label != null)
                        details.Add($"{label} · {condition.display}");
                }
            }

            if (item.IsTag(DurabilityHelper.DURABILITY_TAG))
            {
                int maximum = DurabilityHelper.GetMaxDurability(item);
                if (maximum > 0)
                {
                    string durability = $"{Number(DurabilityHelper.GetRealDurability(item))}/{Number(maximum)}";
                    details.Add(Text($"Echte Haltbarkeit · {durability}", $"True durability · {durability}"));
                }
            }

            if (PlayerStore.HasInstance && item.IsTag(AppraisalHelper.APPRAISAL_ITEM))
            {
                int value = AppraisalHelper.GetCorrectBaseValue(item);
                if (value > 0)
                    details.Add(Text($"Gutachterwert · {Number(value)} Credits", $"Appraisal value · {Number(value)} credits"));
            }
        }

        private static string QualityLabel(string category, string id)
        {
            switch (category)
            {
                case ItemFeatureList.CATEGORY_INGOT_PURITY:
                case ItemFeatureList.CATEGORY_WATER_PURITY:
                case ItemFeatureList.CATEGORY_CHEMICAL_PURITY:
                    return Text("Echte Reinheit", "True purity");
                case ItemFeatureList.CATEGORY_WINE_QUALITY:
                    return Text("Echte Weinqualität", "True wine quality");
                case ItemFeatureList.CATEGORY_FRESHNESS:
                    return Text("Echte Frische", "True freshness");
                case ItemFeatureList.CATEGORY_EQUIPMENT_CONDITION:
                case ItemFeatureList.CATEGORY_OPEN_BOX:
                case ItemFeatureList.CATEGORY_RUSTED:
                case ItemFeatureList.CATEGORY_DENTED:
                case ItemFeatureList.CATEGORY_SHABBY:
                case ItemFeatureList.CATEGORY_SECOND_HAND:
                    return Text("Echter Zustand", "True condition");
                case ItemFeatureList.CATEGORY_EXPIRED:
                case ItemFeatureList.CATEGORY_GENUINE_INJECTOR when id == "expiredInjector":
                    return Text("Haltbarkeitsstatus", "Expiry status");
                default:
                    return null;
            }
        }
    }
}
