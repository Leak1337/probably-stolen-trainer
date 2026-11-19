using System;
using UnityEngine;

namespace ProbablyStolenTrainer
{
    public sealed class TrainerMod : IMod
    {
        private GameObject panel;

        public void Init(ModManifest manifest) { }

        public void OnEnable()
        {
            ModHook.OnGameLoadedNormal += CreatePanel;
            ModHook.OnNewGameStarted += OnNewGame;
        }

        public void OnDisable()
        {
            ModHook.OnGameLoadedNormal -= CreatePanel;
            ModHook.OnNewGameStarted -= OnNewGame;

            if (panel != null)
            {
                panel.SetActive(false);
                UnityEngine.Object.Destroy(panel);
            }
        }

        private void OnNewGame(string scenarioId) => CreatePanel();

        private void CreatePanel()
        {
            if (panel != null)
                return;

            // The panel belongs to the gameplay scene and is destroyed with it.
            panel = new GameObject("Probably Stolen Trainer");
            panel.AddComponent<TrainerPanel>();
        }
    }

    public sealed class TrainerPanel : MonoBehaviour
    {
        private bool visible;
        private string message = "";
        private Rect window = new Rect(24, 64, 320, 232);

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F7))
            {
                visible = !visible;
                message = "";
            }
        }

        private void OnGUI()
        {
            if (!visible || !PlayerStore.HasInstance)
                return;

            int previousDepth = GUI.depth;
            try
            {
                GUI.depth = -100;
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
            GUILayout.Label($"Credits   {store.GetCash():N0}");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+1.000", GUILayout.Height(28)))
                AddCredits(store, 1000);
            if (GUILayout.Button("+10.000", GUILayout.Height(28)))
                AddCredits(store, 10000);
            GUILayout.EndHorizontal();

            GUILayout.Space(8);
            GUILayout.Label($"Wild Favor   {store.wildFavor:N0}");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+100", GUILayout.Height(28)))
                AddFavor(store, 100);
            if (GUILayout.Button("+1.000", GUILayout.Height(28)))
                AddFavor(store, 1000);
            GUILayout.EndHorizontal();

            GUILayout.Space(8);
            GUILayout.Label(message);
            if (GUILayout.Button("Schließen (F7)", GUILayout.Height(26)))
                visible = false;
            GUILayout.EndVertical();

            GUI.DragWindow(new Rect(0, 0, window.width, 20));
        }

        private void AddCredits(PlayerStore store, int amount)
        {
            int added = CapIncrease(store.GetCash(), amount);
            ModHelper.AddCredit(added, false);
            message = added == 0 ? "Credit-Limit erreicht" : $"+{added:N0} Credits";
        }

        private void AddFavor(PlayerStore store, int amount)
        {
            int added = CapIncrease(store.wildFavor, amount);
            store.wildFavor += added;
            message = added == 0 ? "Wild-Favor-Limit erreicht" : $"+{added:N0} Wild Favor";
        }

        private static int CapIncrease(int current, int requested)
        {
            return (int)Math.Min(requested, (long)int.MaxValue - current);
        }
    }
}
