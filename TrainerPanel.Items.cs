using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace ProbablyStolenTrainer
{
    public sealed partial class TrainerPanel
    {
        private const int PageSize = 6;
        private const int MaximumQuantity = 50;

        private List<ItemCatalogEntry> catalog;
        private List<ItemCatalogEntry> matches;
        private ItemCatalogEntry selectedItem;
        private string search = "";
        private string quantity = "1";
        private int page;

        private void ResetItems()
        {
            catalog = null;
            matches = null;
            selectedItem = null;
            search = "";
            quantity = "1";
            page = 0;
        }

        private void DrawItems()
        {
            if (catalog == null)
                LoadCatalog();

            GUILayout.BeginHorizontal();
            GUILayout.Label(Text("Name oder ID", "Name or ID"), GUILayout.Width(95));
            string nextSearch = GUILayout.TextField(search);
            GUILayout.EndHorizontal();
            if (nextSearch != search)
            {
                search = nextSearch;
                FilterItems();
                GUIUtility.ExitGUI();
            }

            int first = page * PageSize;
            int last = Math.Min(first + PageSize, matches.Count);
            for (int index = first; index < last; index++)
            {
                ItemCatalogEntry entry = matches[index];
                string label = entry == selectedItem ? $"> {entry.Name}" : entry.Name;
                if (GUILayout.Button(label, GUILayout.Height(22)))
                {
                    selectedItem = entry;
                    message = "";
                }
            }

            if (matches.Count == 0)
                GUILayout.Label(Text("Keine Treffer", "No results"));

            GUILayout.BeginHorizontal();
            bool previousPage = PageButton(Text("Zurück", "Previous"), page > 0);
            GUILayout.FlexibleSpace();
            if (matches.Count > 0)
                GUILayout.Label($"{Number(first + 1)}–{Number(last)} / {Number(matches.Count)}");
            GUILayout.FlexibleSpace();
            bool nextPage = PageButton(Text("Weiter", "Next"), last < matches.Count);
            GUILayout.EndHorizontal();
            if (previousPage || nextPage)
            {
                page += nextPage ? 1 : -1;
                GUIUtility.ExitGUI();
            }

            GUILayout.Space(6);
            GUILayout.Label(selectedItem?.Id ?? Text("Gegenstand auswählen", "Select an item"));
            GUILayout.BeginHorizontal();
            GUILayout.Label(Text($"Menge (1–{MaximumQuantity})", $"Quantity (1–{MaximumQuantity})"), GUILayout.Width(120));
            quantity = GUILayout.TextField(quantity, GUILayout.Width(60));
            GUILayout.FlexibleSpace();
            if (PageButton(Text("Ins Inventar", "Add to inventory"), selectedItem != null, 140))
                SpawnSelectedItem();
            GUILayout.EndHorizontal();
        }

        private static bool PageButton(string label, bool enabled, float width = 82)
        {
            bool previous = GUI.enabled;
            GUI.enabled = previous && enabled;
            bool clicked = GUILayout.Button(label, GUILayout.Width(width));
            GUI.enabled = previous;
            return clicked;
        }

        private void LoadCatalog()
        {
            StringTable names = LocalizationSettings.StringDatabase.GetTable("Item");
            catalog = DirectoryMaster.GetIdentifierList<GameItem>(null)
                .Where(id => !string.IsNullOrEmpty(id) && !ItemSpawnerHiddenList.IsHidden(id))
                .Distinct(StringComparer.Ordinal)
                .Select(id => new ItemCatalogEntry(id, names?.GetEntry($"item_{id}_name")?.Value))
                .OrderBy(entry => entry.Name, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(entry => entry.Id, StringComparer.Ordinal)
                .ToList();
            FilterItems();
        }

        private void FilterItems()
        {
            string query = search.Trim();
            matches = catalog.Where(entry => entry.Matches(query)).ToList();
            selectedItem = null;
            page = 0;
            message = "";
        }

        private void SpawnSelectedItem()
        {
            if (!BalanceControl.TryParse(quantity, out int requested) || requested < 1 || requested > MaximumQuantity)
            {
                message = Text($"Menge zwischen 1 und {MaximumQuantity} eingeben.", $"Enter a quantity from 1 to {MaximumQuantity}.");
                return;
            }

            EmporiumEntry emporium = EmporiumEntry.Instance;
            GameGridInventory inventory = emporium != null ? emporium.invElement : null;
            if (inventory == null)
            {
                message = Text("Ladeninventar gerade nicht verfügbar.", "Shop inventory is currently unavailable.");
                return;
            }

            string id = selectedItem.Id;
            if (!DirectoryMaster.Has<GameItem>(id))
            {
                message = Text("Der Gegenstand ist nicht mehr verfügbar.", "This item is no longer available.");
                return;
            }

            int added = 0;
            try
            {
                for (; added < requested; added++)
                {
                    GameItem item = DirectoryMaster.Item(id, true);
                    try
                    {
                        if (item.unitCount != 1)
                            item.SetUnitCount(1);

                        // TryAcceptAll liefert den Rest / TryAcceptAll returns the remainder.
                        if (GraphUtils.TryAcceptAll(inventory, item, 1) != 0)
                            break;
                    }
                    finally
                    {
                        GraphUtils.DestroyIfUnplaced(item, "Probably Stolen Trainer");
                    }
                }
            }
            catch (Exception failure)
            {
                UnityEngine.Debug.LogException(failure);
                message = Text($"{added}/{requested} hinzugefügt. Fehler im Spielprotokoll.", $"Added {added}/{requested}. See the game log for the error.");
                return;
            }

            message = added == requested
                ? Text($"{added}× hinzugefügt.", $"Added {added}.")
                : Text($"{added}/{requested} hinzugefügt. Kein passender Platz.", $"Added {added}/{requested}. No suitable space.");
        }
    }

    internal sealed class ItemCatalogEntry
    {
        internal ItemCatalogEntry(string id, string localizedName)
        {
            Id = id;
            Name = string.IsNullOrWhiteSpace(localizedName) ? id.Replace('_', ' ') : localizedName;
        }

        internal string Id { get; }
        internal string Name { get; }

        internal bool Matches(string query) =>
            Id.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 ||
            CultureInfo.InvariantCulture.CompareInfo.IndexOf(Name, query, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0;
    }
}
