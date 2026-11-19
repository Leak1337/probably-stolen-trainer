using System;
using System.Globalization;
using UnityEngine;

namespace ProbablyStolenTrainer
{
    public sealed class TrainerMod : IMod
    {
        private TrainerPanel panel;
        private InspectionOptions inspection;

        public void Init(ModManifest manifest)
        {
            inspection = new InspectionOptions(ModHelper.GetConfig(manifest.ID));
        }

        public void OnEnable()
        {
            ModHook.OnGameLoadedNormal += CreatePanel;
            ModHook.OnNewGameStarted += OnNewGame;
            ModHook.OnCreateTooltipLate += inspection.AppendItemDetails;
        }

        public void OnDisable()
        {
            ModHook.OnGameLoadedNormal -= CreatePanel;
            ModHook.OnNewGameStarted -= OnNewGame;
            ModHook.OnCreateTooltipLate -= inspection.AppendItemDetails;

            if (panel != null)
            {
                panel.gameObject.SetActive(false);
                UnityEngine.Object.Destroy(panel.gameObject);
            }
        }

        private void OnNewGame(string scenarioId) => CreatePanel();

        private void CreatePanel()
        {
            if (panel == null)
            {
                // Gehört zur Spielszene / Owned by the gameplay scene.
                var panelObject = new GameObject("Probably Stolen Trainer");
                panel = panelObject.AddComponent<TrainerPanel>();
                panel.Inspection = inspection;
            }

            panel.ResetSession();
        }
    }

    public sealed partial class TrainerPanel : MonoBehaviour
    {
        private static readonly CultureInfo German = CultureInfo.GetCultureInfo("de-DE");
        private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en-US");

        private static bool? german;
        private readonly BalanceControl credits = new BalanceControl();
        private readonly BalanceControl favor = new BalanceControl();
        private bool visible;
        private int selectedTab;
        private string message = "";
        private Rect window = new Rect(24, 40, 420, 425);

        internal InspectionOptions Inspection { get; set; }

        private void Awake() => german ??= Application.systemLanguage == SystemLanguage.German;

        internal void ResetSession()
        {
            credits.Reset();
            favor.Reset();
            ResetItems();
            ResetStoreTools();
            selectedTab = 0;
            visible = false;
            message = "";
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F7))
            {
                visible = !visible;
                message = "";
            }
        }

        private void LateUpdate()
        {
            if ((!credits.Minimum.HasValue && !favor.Minimum.HasValue) || !PlayerStore.HasInstance)
                return;

            PlayerStore store = PlayerStore.Instance;
            store.playerCash = credits.ApplyMinimum(store.playerCash);
            store.wildFavor = favor.ApplyMinimum(store.wildFavor);
        }

        private void OnGUI()
        {
            if (!PlayerStore.HasInstance)
                return;

            int previousDepth = GUI.depth;
            try
            {
                GUI.depth = -100;
                Inspection.DrawCustomer(PlayerStore.Instance);
                if (visible)
                    window = GUI.Window(GetInstanceID(), window, DrawPanel, "Probably Stolen Trainer");
            }
            finally
            {
                GUI.depth = previousDepth;
            }
        }

        private void DrawPanel(int id)
        {
            PlayerStore store = PlayerStore.Instance;

            GUILayout.BeginVertical();
            GUILayout.Space(8);
            GUILayout.BeginHorizontal();
            GUILayout.Label(Text("Sprache", "Language"));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(Text("EN", "DE"), GUILayout.Width(50)))
            {
                german = german != true;
                message = "";
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            int nextTab = GUILayout.Toolbar(selectedTab, new[]
            {
                Text("Guthaben", "Balances"), Text("Gegenstände", "Items"), Text("Analyse", "Inspection"), Text("Laden", "Shop")
            });
            if (nextTab != selectedTab)
            {
                selectedTab = nextTab;
                message = "";
                GUIUtility.ExitGUI();
            }

            GUILayout.Space(8);
            switch (selectedTab)
            {
                case 1:
                    DrawItems();
                    break;
                case 2:
                    Inspection.DrawOptions();
                    break;
                case 3:
                    DrawStoreTools(store);
                    break;
                default:
                    DrawBalance("Credits", ref store.playerCash, credits, 1000, 10000);
                    GUILayout.Space(10);
                    DrawBalance("Wild Favor", ref store.wildFavor, favor, 100, 1000);
                    break;
            }

            GUILayout.Space(8);
            GUILayout.Label(message);
            if (GUILayout.Button(Text("Schließen (F7)", "Close (F7)"), GUILayout.Height(26)))
                visible = false;
            GUILayout.EndVertical();

            GUI.DragWindow(new Rect(0, 0, window.width, 20));
        }

        private void DrawBalance(string name, ref int balance, BalanceControl control, int small, int large)
        {
            GUILayout.Label($"{name}   {Number(balance)}");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button($"+{Number(small)}", GUILayout.Height(26)))
                AddBalance(name, ref balance, control, small);
            if (GUILayout.Button($"+{Number(large)}", GUILayout.Height(26)))
                AddBalance(name, ref balance, control, large);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(Text("Neuer Stand", "New balance"), GUILayout.Width(105));
            control.Input = GUILayout.TextField(control.Input);
            if (GUILayout.Button(Text("Setzen", "Set"), GUILayout.Width(75)))
                SetBalance(name, ref balance, control);
            if (EnabledButton(Text("Rückgängig", "Undo"), control.HasUndo, 92))
                UndoBalance(name, ref balance, control);
            GUILayout.EndHorizontal();

            if (control.Minimum is int minimum)
            {
                if (GUILayout.Button(Text($"Mindeststand {Number(minimum)} · Ausschalten", $"Minimum {Number(minimum)} · Turn off")))
                    control.Minimum = null;
            }
            else if (GUILayout.Button(Text("Aktuellen Stand als Minimum halten", "Keep current balance as minimum")))
            {
                control.Minimum = balance;
            }
        }

        private void AddBalance(string name, ref int balance, BalanceControl control, int amount)
        {
            int next = BalanceControl.Increase(balance, amount);
            int added = next - balance;
            control.ApplyManualChange(ref balance, next);
            message = added == 0 ? Text("Obergrenze erreicht", "Upper limit reached") : $"+{Number(added)} {name}";
        }

        private void SetBalance(string name, ref int balance, BalanceControl control)
        {
            if (!BalanceControl.TryParse(control.Input, out int value))
            {
                message = Text("Ganze Zahl von 0 bis 2.147.483.647 eingeben.", "Enter an integer from 0 to 2,147,483,647.");
                return;
            }

            if (control.Minimum is int minimum && value < minimum)
            {
                message = Text("Schalte zuerst den Mindeststand aus.", "Turn off the minimum first.");
                return;
            }

            control.ApplyManualChange(ref balance, value);
            message = Text($"{name} auf {Number(value)} gesetzt", $"{name} set to {Number(value)}");
        }

        private void UndoBalance(string name, ref int balance, BalanceControl control)
        {
            switch (control.Undo(ref balance))
            {
                case BalanceUndoResult.Applied:
                    message = Text($"{name} rückgängig · {Number(balance)}", $"{name} undone · {Number(balance)}");
                    break;
                case BalanceUndoResult.Minimum:
                    message = Text("Schalte zuerst den Mindeststand aus.", "Turn off the minimum first.");
                    break;
                case BalanceUndoResult.Limit:
                    message = Text("Rückgängig würde den Wertebereich überschreiten.", "Undo would exceed the balance range.");
                    break;
            }
        }

        private static bool EnabledButton(string label, bool enabled, float width)
        {
            bool previous = GUI.enabled;
            GUI.enabled = previous && enabled;
            try
            {
                return GUILayout.Button(label, GUILayout.Width(width));
            }
            finally
            {
                GUI.enabled = previous;
            }
        }

        internal static string Text(string de, string en) => IsGerman ? de : en;

        internal static string Number(int value) => value.ToString("N0", IsGerman ? German : English);

        private static string DecimalNumber(double value) => value.ToString("0.##", IsGerman ? German : English);

        private static bool IsGerman => german ?? (Application.systemLanguage == SystemLanguage.German);
    }

    internal sealed class BalanceControl
    {
        private long lastChange;
        internal string Input = "";
        internal int? Minimum;
        internal bool HasUndo => lastChange != 0;

        internal void Reset()
        {
            Input = "";
            Minimum = null;
            lastChange = 0;
        }

        internal void ApplyManualChange(ref int balance, int newBalance)
        {
            if (newBalance == balance)
                return;

            lastChange = (long)newBalance - balance;
            balance = newBalance;
        }

        internal BalanceUndoResult Undo(ref int balance)
        {
            if (!HasUndo)
                return BalanceUndoResult.Unavailable;

            long previous = balance - lastChange;
            if (previous < int.MinValue || previous > int.MaxValue)
                return BalanceUndoResult.Limit;
            if (Minimum is int minimum && previous < minimum)
                return BalanceUndoResult.Minimum;

            balance = (int)previous;
            lastChange = 0;
            return BalanceUndoResult.Applied;
        }

        internal int ApplyMinimum(int balance) => Math.Max(balance, Minimum ?? balance);

        internal static int Increase(int balance, int amount) => (int)Math.Min(int.MaxValue, (long)balance + amount);

        internal static bool TryParse(string input, out int value) =>
            int.TryParse(input, NumberStyles.None, CultureInfo.InvariantCulture, out value);
    }

    internal enum BalanceUndoResult
    {
        Unavailable,
        Applied,
        Minimum,
        Limit
    }
}
