using UnityEngine;
using UnityEngine.InputSystem;

namespace RogZombie.TestEngine
{
    public sealed class TestHUD : MonoBehaviour
    {
        private TestEngineBootstrap world;
        private PlayerRuntime subscribed;
        private string notice;
        private float noticeUntil;
        private GUIStyle bannerStyle;
        public static bool PointerOverControls
        {
            get
            {
                if (Mouse.current == null) return false;
                float top = Screen.height - Mouse.current.position.ReadValue().y;
                return top < 105f;
            }
        }

        private void Awake() => world = GetComponent<TestEngineBootstrap>();
        private void Update()
        {
            if (world.Player == subscribed) return;
            if (subscribed != null) subscribed.Notice -= ShowNotice;
            subscribed = world.Player;
            if (subscribed != null) subscribed.Notice += ShowNotice;
        }
        private void ShowNotice(string message) { notice = message; noticeUntil = Time.unscaledTime + 4f; }

        private void OnGUI()
        {
            if (world == null) return;
            GUI.Box(new Rect(8, 8, Screen.width - 16, 94), "TEST ENGINE #2 • WASD / mouse / LMB • Cambio PG = nuova prova");
            if (world.Failure != null)
            {
                GUI.Label(new Rect(20, 110, Screen.width - 40, 80), world.Failure);
                return;
            }
            var player = world.Player;
            if (player == null) return;
            var progress = player.GetComponent<ExperienceProgression>();
            var inventory = player.GetComponent<BonusInventory>();
            GUI.enabled = !world.Loading && progress.PendingChoices == 0;
            float width = Mathf.Min(70f, (Screen.width - 120f) / 8f);
            for (int i = 0; i < 8; i++)
                if (GUI.Button(new Rect(20 + i * (width + 3f), 32, width, 25), "PG0" + (i + 1))) world.Restart(i);
            if (GUI.Button(new Rect(Screen.width - 90, 32, 70, 25), "Riprova")) world.Restart(world.StartingPG);
            GUI.enabled = true;
            string threshold = progress.NextThreshold > 0 ? progress.NextThreshold.ToString() : "tabella conclusa";
            GUI.Label(new Rect(20, 65, Screen.width - 40, 26),
                $"{player.Definition.PlayerId}  HP {player.Actor.CurrentHP:0.#}/{player.Actor.Stats.HP:0.#}  LVL {progress.Level}  EXP {progress.Experience}/{threshold}  G {world.Gold}");
            if (world.Settings.ShowSpawnCounters && world.Spawns != null)
                GUI.Box(new Rect(Screen.width - 270, 110, 250, 78),
                    $"MOB ATTIVI: {world.Spawns.Alive} / {world.Spawns.MaxSimultaneous}\nTOTAL SPAWNED: {world.Spawns.TotalSpawned} / {world.Settings.TotalMobs}\nTOTAL DEAD: {world.Spawns.Killed}");
            if (world.Loading) GUI.Box(new Rect(Screen.width / 2f - 170, Screen.height / 2f - 30, 340, 60), "Caricamento AREA / NavMesh / FIRST SPAWN…");
            if (!world.Loading && !player.Actor.IsActive && progress.PendingChoices == 0)
                GUI.Box(new Rect(Screen.width / 2f - 190, Screen.height / 2f - 35, 380, 70), "PG DOWN — prova terminata\nUsa Riprova o scegli un altro PG.");
            if (world.Spawns != null && world.Spawns.Killed == world.Settings.TotalMobs)
                GUI.Box(new Rect(Screen.width / 2f - 160, 110, 320, 40), "AREA COMPLETATA");
            if (Time.unscaledTime < noticeUntil) GUI.Box(new Rect(20, Screen.height - 70, 440, 40), notice);
            GUI.Label(new Rect(20, Screen.height - 28, Screen.width - 40, 25), "BONUS: acquisizione e upgrade dati; effetti non attivi (regole GDD da completare).");
            if (progress.Choices == null) return;
            GUI.Box(new Rect(0, 105, Screen.width, Screen.height - 105), "");
            GUI.Label(new Rect(30, 125, Screen.width - 60, 35), $"LVL UP — scegli 1 BONUS • scelte rimanenti: {progress.PendingChoices}");
            if (bannerStyle == null) bannerStyle = new GUIStyle(GUI.skin.button) { wordWrap = true, fontSize = 16 };
            float cardWidth = (Screen.width - 80f) / 3f;
            for (int i = 0; i < progress.Choices.Length; i++)
                if (GUI.Button(new Rect(20 + i * (cardWidth + 20), Screen.height / 2f - 75, cardWidth, 150), inventory.Label(progress.Choices[i]), bannerStyle))
                { progress.Choose(i); break; }
        }

        private void OnDestroy() { if (subscribed != null) subscribed.Notice -= ShowNotice; }
    }
}
