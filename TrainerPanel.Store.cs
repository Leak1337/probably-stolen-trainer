using System;
using System.Globalization;
using UnityEngine;

namespace ProbablyStolenTrainer
{
    public sealed partial class TrainerPanel
    {
        private const int MinReputation = -200;
        private const int MaxReputation = 200;
        private int factionIndex;
        private string reputationInput = "";

        private void ResetStoreTools()
        {
            factionIndex = 0;
            reputationInput = "";
        }

        private void DrawStoreTools(PlayerStore store)
        {
            DrawReputation(store);
            GUILayout.Space(16);
            DrawScavenging(store);
        }

        private void DrawReputation(PlayerStore store)
        {
            GUILayout.Label(Text("Fraktionsruf", "Faction reputation"));
            var factions = store.storeReputations;
            if (factions == null || factions.Count == 0)
            {
                GUILayout.Label(Text("Für diesen Spielstand sind keine Fraktionen geladen.", "No factions are loaded for this run."));
                return;
            }

            factionIndex = Math.Min(factionIndex, factions.Count - 1);
            StoreReputation faction = factions[factionIndex];
            GUILayout.BeginHorizontal();
            bool previous = EnabledButton("<", factionIndex > 0, 36);
            GUILayout.Label(faction == null ? Text("Fraktion nicht verfügbar", "Faction unavailable") : FactionName(faction));
            GUILayout.FlexibleSpace();
            GUILayout.Label($"{factionIndex + 1}/{factions.Count}");
            bool next = EnabledButton(">", factionIndex < factions.Count - 1, 36);
            GUILayout.EndHorizontal();
            if (previous || next)
            {
                factionIndex += next ? 1 : -1;
                reputationInput = "";
                message = "";
                GUIUtility.ExitGUI();
            }

            if (faction == null)
                return;

            double current = faction.GetReputationExact();
            GUILayout.Label(Text($"Aktueller Ruf · {DecimalNumber(current)}", $"Current reputation · {DecimalNumber(current)}"));

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("−10"))
                SetReputation(faction, current - 10);
            if (GUILayout.Button("+10"))
                SetReputation(faction, current + 10);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(Text("Neuer Ruf (−200 bis 200)", "New reputation (−200 to 200)"));
            reputationInput = GUILayout.TextField(reputationInput, GUILayout.Width(75));
            if (GUILayout.Button(Text("Setzen", "Set"), GUILayout.Width(75)))
            {
                if (TryParseReputation(reputationInput, out int value))
                    SetReputation(faction, value);
                else
                    message = Text("Ganze Zahl von −200 bis 200 eingeben.", "Enter a whole number from −200 to 200.");
            }
            GUILayout.EndHorizontal();
        }

        private static bool TryParseReputation(string input, out int value) =>
            int.TryParse(input?.Trim().Replace('−', '-'), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value) &&
            value >= MinReputation && value <= MaxReputation;

        private static string FactionName(StoreReputation faction) =>
            string.IsNullOrWhiteSpace(faction.factionDisplay) ? faction.factionId : faction.factionDisplay;

        private void SetReputation(StoreReputation faction, double value)
        {
            faction.SetReputation(Math.Clamp(value, MinReputation, MaxReputation));
            string amount = DecimalNumber(faction.GetReputationExact());
            message = Text($"Ruf gesetzt · {amount}", $"Reputation set · {amount}");
        }

        private void DrawScavenging(PlayerStore store)
        {
            GUILayout.Label(Text("Schrottplatz", "Dumping Grounds"));
            MapUIManager map = MapUIManager.Instance;
            if (map == null || !map.isOutside || map.Current?.Id != AfterhourLocation.DUMPING_GROUND)
            {
                GUILayout.Label(Text("Suchversuche können vor Ort aufgefüllt werden.", "Refill scavenging attempts while at the Dumping Grounds."));
                return;
            }

            if (store.healthData?.isWoundedFresh == true)
            {
                GUILayout.Label(Text("Die aktuelle Verletzung verhindert das Suchen.", "Your current injury prevents scavenging."));
                return;
            }

            int remaining = Math.Max(0, ScavHelper.GetScavTimeLeft());
            GUILayout.Label(Text($"Übrige Suchversuche · {Number(remaining)}", $"Remaining scavenging attempts · {Number(remaining)}"));
            if (EnabledButton(Text("Suchversuche auffüllen", "Refill scavenging attempts"), store.scavengingAttempts > 0, 220))
            {
                ScavHelper.ResetScavenging();
                map.UpdateScavUI();
                message = Text("Suchversuche aufgefüllt.", "Scavenging attempts refilled.");
            }
        }
    }
}
