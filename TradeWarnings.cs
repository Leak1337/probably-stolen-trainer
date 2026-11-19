using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using static ProbablyStolenTrainer.TrainerPanel;

namespace ProbablyStolenTrainer
{
    internal static class TradeWarnings
    {
        private const int PageSize = 4;
        private static object pending;
        private static bool resuming;

        internal static ConfigEntry<bool> Option { get; private set; }

        internal static void Initialize(ModConfig config)
        {
            Option = config.Define("warn_risky_trade", true, "Inspection",
                () => Text("Vor riskanter Ware beim Handel warnen", "Warn about risky goods when trading"));
        }

        internal static void Reset() => pending = null;

        internal static void Stop()
        {
            Reset();
            Option = null;
        }

        [HarmonyPatch(typeof(NegociationUIManager), nameof(NegociationUIManager.OnOfferAcceptClick))]
        private static class SingleOfferPatch
        {
            private static bool Prefix(NegociationUIManager __instance)
            {
                if (!TryGetTradeStore(out PlayerStore store) || !TryGetSingleOffer(store, out GameItem item, out bool buying))
                    return true;

                List<GameItem> ReadCurrent() => TryGetSingleOffer(store, out GameItem current, out bool currentBuying) && currentBuying == buying
                    ? new List<GameItem> { current } : new List<GameItem>();

                return AllowOrWarn(__instance, store, new[] { item }, buying, ReadCurrent, _ => __instance.OnOfferAcceptClick());
            }
        }

        [HarmonyPatch(typeof(NegociationUIManager), nameof(NegociationUIManager.SellMultiple))]
        private static class BulkSalePatch
        {
            private static bool Prefix(NegociationUIManager __instance, List<GameItem> __0, bool __1)
            {
                if (!TryGetTradeStore(out PlayerStore store) || __0 == null || __0.Count == 0)
                    return true;

                return AllowOrWarn(__instance, store, __0, false, __instance.GetSellableItem, items => __instance.SellMultiple(items, __1));
            }
        }

        private static bool TryGetTradeStore(out PlayerStore store)
        {
            store = null;
            if (resuming || Option?.Value != true || !PlayerStore.HasInstance || NegociationUIManager.IsTradeLocked())
                return false;

            store = PlayerStore.Instance;
            return store.currentClientInstance?.GetClientBlueprint()?.isBusinessComplete == false;
        }

        private static bool TryGetSingleOffer(PlayerStore store, out GameItem item, out bool buying)
        {
            item = store.GetCurrentlyNegociatedItem();
            buying = false;
            if (item == null)
                return false;

            bool owned = GeneralHelper.IsItemOwned(item);
            StoreClient.ClientIntent intent = store.GetCurrentClientIntent();
            buying = !owned && (intent == StoreClient.ClientIntent.SELL || intent == StoreClient.ClientIntent.SELLNBUY);
            if (buying)
                return store.CanBuyItemFromTable();

            // Budgetfreigaben laufen über die Sammelbestätigung / Budget overrides use the batch confirmation.
            return owned && (intent == StoreClient.ClientIntent.BUY || intent == StoreClient.ClientIntent.SELLNBUY) &&
                store.CanSellItemFromTable(false);
        }

        private static bool AllowOrWarn(NegociationUIManager manager, PlayerStore store, IReadOnlyList<GameItem> items,
            bool buying, Func<List<GameItem>> readCurrent, Action<List<GameItem>> proceed)
        {
            GenericConfirmationUIManager dialog = GenericConfirmationUIManager.Instance;
            if (dialog == null || dialog.panel == null)
                return true;
            if (pending != null && dialog.panel.activeSelf)
                return false;

            List<TradeLine> lines = ReadOffer(items);
            TradeLine[] warnings = lines.Where(line => line.Risks.Length != 0).ToArray();
            if (warnings.Length == 0)
                return true;
            if (dialog.panel.activeSelf)
                return false;

            var token = new object();
            pending = token;
            StoreClientInstance client = store.currentClientInstance;
            List<GameItem> originalOrder = items.ToList();

            void Confirm()
            {
                if (pending != token)
                    return;
                Reset();

                if (manager == null || !manager.IsOpen || !TryGetTradeStore(out PlayerStore currentStore) ||
                    !ReferenceEquals(currentStore, store) || !ReferenceEquals(currentStore.currentClientInstance, client))
                    return;

                List<GameItem> current = readCurrent();
                if (!SameOffer(lines, ReadOffer(current)))
                {
                    StoreUIManager notice = StoreUIManager.Instance;
                    if (notice != null)
                        notice.Notify(Text("Das Angebot hat sich geändert. Bestätige es erneut.", "The offer changed. Confirm it again."), "sec");
                    return;
                }

                resuming = true;
                try
                {
                    proceed(originalOrder);
                }
                finally
                {
                    resuming = false;
                }
            }

            ShowWarning(dialog, token, warnings, buying, 0, Confirm);
            return false;
        }

        private static void ShowWarning(GenericConfirmationUIManager dialog, object token, TradeLine[] warnings, bool buying,
            int offset, Action confirm)
        {
            var entries = new List<string> { buying ? Text("Riskanter Kauf", "Risky purchase") : Text("Riskanter Verkauf", "Risky sale") };
            int end = Math.Min(offset + PageSize, warnings.Length);
            for (int index = offset; index < end; index++)
            {
                TradeLine line = warnings[index];
                string name = line.Item.GetDisplayName(true) ?? line.Item.identifier ?? Text("Unbekannter Gegenstand", "Unknown item");
                name = name.Replace('<', '‹').Replace('>', '›').Replace('\n', ' ').Replace('\r', ' ');
                entries.Add($"{name} ×{Number(line.Quantity)} · {line.Risks}");
            }

            bool hasMore = end < warnings.Length;
            if (warnings.Length > PageSize)
                entries.Add($"{offset + 1}–{end} / {warnings.Length}");

            // Der native Dialog schließt nach dem Callback / The native dialog closes after its callback.
            Action onConfirm = hasMore
                ? () => ModHelper.StartCoroutine(Option.Config.ModId, ShowNextPage(dialog, token, warnings, buying, end, confirm))
                : confirm;
            dialog.OpenPanel(string.Join("\n", entries), hasMore ? Text("Weiter", "Next") : Text("Trotzdem handeln", "Trade anyway"),
                Text("Abbrechen", "Cancel"), onConfirm, () => { if (pending == token) Reset(); });
        }

        private static IEnumerator ShowNextPage(GenericConfirmationUIManager dialog, object token, TradeLine[] warnings,
            bool buying, int offset, Action confirm)
        {
            yield return null;
            if (pending == token && dialog != null && dialog.panel != null && !dialog.panel.activeSelf)
                ShowWarning(dialog, token, warnings, buying, offset, confirm);
        }

        private static List<TradeLine> ReadOffer(IReadOnlyList<GameItem> roots)
        {
            var lines = new List<TradeLine>();
            var seen = new HashSet<GameItem>();
            foreach (GameItem root in roots)
            {
                if (root == null || !seen.Add(root))
                    continue;

                lines.Add(new TradeLine(root, null, root.GetNegociatedValue()));
                foreach (GameItem child in root.FindAllChildItems())
                {
                    if (child != null && seen.Add(child))
                        lines.Add(new TradeLine(child, root, 0));
                }
            }

            return lines;
        }

        internal static bool SameOffer(IReadOnlyList<TradeLine> previous, IReadOnlyList<TradeLine> current)
        {
            if (previous.Count != current.Count)
                return false;

            var byItem = current.ToDictionary(line => line.Item);
            foreach (TradeLine before in previous)
            {
                if (!byItem.TryGetValue(before.Item, out TradeLine after) || !ReferenceEquals(before.Item, after.Item) ||
                    !ReferenceEquals(before.Root, after.Root) || before.Quantity != after.Quantity ||
                    before.Price != after.Price || before.Risks != after.Risks)
                    return false;
            }
            return true;
        }

        internal sealed class TradeLine
        {
            internal TradeLine(GameItem item, GameItem root, long price)
            {
                Item = item;
                Root = root;
                Quantity = item.unitCount;
                Price = price;
                var risks = new List<string>();
                InspectionOptions.AddAuthenticity(risks, item, includeGenuine: false);
                Risks = string.Join(", ", risks);
            }

            internal GameItem Item { get; }
            internal GameItem Root { get; }
            internal int Quantity { get; }
            internal long Price { get; }
            internal string Risks { get; }
        }
    }
}
