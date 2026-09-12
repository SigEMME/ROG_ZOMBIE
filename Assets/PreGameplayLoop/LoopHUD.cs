using UnityEngine;

namespace RogZombie.PreGameplayLoop
{
    public sealed class LoopHUD : MonoBehaviour
    {
        private LoopSession loop;
        private void Awake() => loop = GetComponent<LoopSession>();

        private void OnGUI()
        {
            GUI.Box(new Rect(8, 8, Screen.width - 16, 94), "Pre gameplay loop prototype | WASD / Mouse / LMB");
            GUI.Label(new Rect(20, 30, Screen.width - 160, 24), $"AREA {loop.AreaIndex + 1}/2 | {loop.State} | G {loop.Gold}");
            if (GUI.Button(new Rect(Screen.width - 140, 32, 115, 26), "Riprova test"))
                loop.RestartTest();
            if (loop.Player != null)
                GUI.Label(new Rect(20, 58, Screen.width - 40, 26), $"PG01 HP {loop.Player.Actor.CurrentHP:0.#}/{loop.Player.Actor.Stats.HP:0.#} | CD REDUCTION {loop.CdReduction}");
            if (loop.Spawns != null)
                GUI.Label(new Rect(20, 110, Screen.width - 40, 24), $"MOB {loop.Spawns.Alive}/{loop.Spawns.MaxSimultaneous} | Generati {loop.Spawns.TotalSpawned}/{loop.Settings.TotalMobs} | Morti {loop.Spawns.Killed}");
            GUI.Label(new Rect(20, Screen.height - 34, Screen.width - 40, 28), "Slice: solo ZOMB01; EXP/LVL, abilita e passive esclusi; geometria provvisoria.");
            if (loop.State == LoopState.AreaComplete)
            {
                Vector3 view = loop.GameCamera.WorldToViewportPoint(loop.Exit.transform.position);
                bool visible = view.z > 0 && view.x >= 0 && view.x <= 1 && view.y >= 0 && view.y <= 1;
                GUI.Label(new Rect(20, 140, Screen.width - 40, 30), "AREA COMPLETATA — raggiungi l'USCITA verde (raggio 4 m)");
                if (!visible)
                {
                    Vector2 delta = loop.Exit.transform.position - loop.Player.transform.position;
                    float angle = Mathf.Atan2(-delta.y, delta.x) * Mathf.Rad2Deg;
                    var matrix = GUI.matrix;
                    var pivot = new Vector2(Screen.width / 2f, 190);
                    GUIUtility.RotateAroundPivot(angle, pivot);
                    GUI.Label(new Rect(pivot.x - 12, pivot.y - 12, 50, 30), "-->");
                    GUI.matrix = matrix;
                }
            }
            if (loop.Loading) GUI.Box(new Rect(30, 180, Screen.width - 60, 60), "Caricamento AREA / FIRST SPAWN off-screen...");
            if (loop.State == LoopState.Error) GUI.Box(new Rect(30, 180, Screen.width - 60, 80), loop.Failure);
            if (loop.State == LoopState.Defeat) GUI.Box(new Rect(30, 180, Screen.width - 60, 60), "SCONFITTA: PG01 DOWN, nessun PG attivo. Riprova test.");
            if (loop.State == LoopState.Finished) GUI.Box(new Rect(30, 180, Screen.width - 60, 60), "Due cicli completati. Fine del test tecnico; Riprova test per una nuova prova.");
            if (loop.State != LoopState.Bonus) return;
            GUI.Box(new Rect(10, 140, Screen.width - 20, 320), "BONUS FINE AREA — scegli una STAT, poi conferma");
            float width = (Screen.width - 60) / 3f;
            for (int i = 0; i < 3; i++)
                if (GUI.Button(new Rect(20 + i * (width + 10), 185, width, 65),
                    (loop.Selected == i ? "[X] " : "") + AreaStatBonus.Labels[(int)loop.Choices[i]])) loop.SelectBonus(i);
            GUI.enabled = false;
            GUI.Button(new Rect(20, 265, (Screen.width - 50) / 2f, 60), "ABILITA BONUS — fuori scope");
            GUI.Button(new Rect(30 + (Screen.width - 50) / 2f, 265, (Screen.width - 50) / 2f, 60), "ABILITA BONUS — fuori scope");
            GUI.enabled = loop.Selected >= 0;
            if (GUI.Button(new Rect(20, 350, Screen.width - 40, 60), "Conferma")) loop.ConfirmBonus();
            GUI.enabled = true;
        }
    }
}
