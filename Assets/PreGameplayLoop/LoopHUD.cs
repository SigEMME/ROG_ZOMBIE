using UnityEngine;

namespace RogZombie.PreGameplayLoop
{
    public sealed class LoopHUD : MonoBehaviour
    {
        private LoopSession loop;
        private void Awake() => loop = GetComponent<LoopSession>();

        private void OnGUI()
        {
            GUI.Box(new Rect(8, 8, Screen.width - 16, 94), "Pre gameplay loop prototype | WASD / Mouse / LMB / Q");
            GUI.Label(new Rect(20, 30, Screen.width - 160, 24), $"AREA {loop.AreaIndex + 1}/2 | {loop.State} | G {loop.Gold}");
            if (GUI.Button(new Rect(Screen.width - 140, 32, 115, 26), "Riprova test"))
                loop.RestartTest();
            if (loop.Player != null)
                GUI.Label(new Rect(20, 58, Screen.width - 40, 26), $"{loop.Player.Definition.PlayerId} HP {loop.Player.Actor.CurrentHP:0.#}/{loop.Player.Actor.Stats.HP:0.#} | CD REDUCTION {loop.CdReduction}");
            if (loop.Spawns != null)
                GUI.Label(new Rect(20, 110, Screen.width - 40, 24), $"MOB {loop.Spawns.Alive}/{loop.Spawns.MaxSimultaneous} | Generati {loop.Spawns.TotalSpawned}/{loop.Settings.TotalMobs} | Morti {loop.Spawns.Killed}");
            if (loop.Ability != null)
                GUI.Label(new Rect(20, Screen.height - 60, Screen.width - 40, 26),
                    $"Q — {loop.Ability.Selected.ToString().ToUpperInvariant()} | CD {loop.Ability.CooldownRemaining:0.0} s" +
                    (loop.Ability.Selected == PG01Ability.Barriera ? " | Tieni Q: anteprima, rilascia: piazza" : ""));
            if (loop.Passive != null && loop.Player != null)
                GUI.Label(new Rect(20, Screen.height - 86, Screen.width - 40, 26),
                    $"{loop.Passive.Label} | {(loop.Passive.EffectActive ? "ATTIVA" : "INATTIVA")} | " +
                    $"ATK {loop.Player.Actor.EffectiveStats.ATK:0.##} | DEF {loop.Player.Actor.EffectiveStats.DEF - 100:0.##}%");
            if (loop.PG02Ability != null)
                GUI.Label(new Rect(20, Screen.height - 60, Screen.width - 40, 26),
                    $"Q — {loop.PG02Ability.Label} | CD {loop.PG02Ability.CooldownRemaining:0.0} s | DURATA {loop.PG02Ability.ActiveRemaining:0.0} s");
            if (loop.PG02Passive != null && loop.Player != null)
            {
                var passive = loop.PG02Passive;
                var stats = loop.Player.Actor.EffectiveStats;
                GUI.Label(new Rect(20, Screen.height - 110, Screen.width - 40, 26),
                    $"{passive.Label} | {(passive.EffectActive ? "ATTIVA" : "INATTIVA")} | KILL {passive.Kills}" +
                    (passive.Selected == PG02Passive.Passiva2 ? $" | RAGE {passive.KillsTowardRage}/15 — {passive.RageRemaining:0.0} s" : ""));
                GUI.Label(new Rect(20, Screen.height - 86, Screen.width - 40, 26),
                    $"ATK {stats.ATK:0.##} | ATK SPD {stats.AttackSpeed:0.##} ({stats.AttackSpeed / 100f:0.##}/s) | DEF {stats.DEF - 100:0.##}%");
            }
            if (loop.PG03Ability != null && loop.Player != null)
            {
                GUI.Label(new Rect(20, Screen.height - 60, Screen.width - 40, 26),
                    $"Q — {loop.PG03Ability.Label} | CD {loop.PG03Ability.CooldownRemaining:0.0} s | DURATA {loop.PG03Ability.ActiveRemaining:0.0} s");
                GUI.Label(new Rect(20, Screen.height - 86, Screen.width - 40, 26),
                    $"{loop.PG03Passive.Label} | AUTOMATICA | ATK {loop.Player.Actor.EffectiveStats.ATK:0.##} | MOVE SPD {loop.Player.Actor.Stats.MoveSpeed:0.##}");
            }
            if (loop.PG08Ability != null)
            {
                var ability = loop.PG08Ability; var passive = loop.PG08Passive; var stats = loop.Player.Actor.EffectiveStats;
                GUI.Label(new Rect(20, Screen.height - 60, Screen.width - 40, 26),
                    $"Q — {ability.Label} | CD {ability.CooldownRemaining:0.0} s | DURATA {ability.DurationRemaining:0.0} s");
                GUI.Label(new Rect(20, Screen.height - 86, Screen.width - 40, 26),
                    $"{passive.Label} | {(passive.EffectActive ? "ATTIVA" : "INATTIVA")} | " +
                    (passive.Selected == PG08Passive.Tenacia ? $"HIT {passive.Hits}/150 | BONUS DEF +{passive.TenacityBonus:0.0}%" : $"FUOCO {passive.ContinuousFire:0.0}/5 s") +
                    $" | ATK {stats.ATK:0.##} | DEF {stats.DEF - 100:0.##}% | MOVE SPD {stats.MoveSpeed:0.##}");
            }
            if (loop.PG07Ability != null)
            {
                var ability = loop.PG07Ability;
                GUI.Label(new Rect(20, Screen.height - 60, Screen.width - 40, 26),
                    $"Q — {ability.Label} | CD {ability.CooldownRemaining:0.0} s | FRECCE {ability.RainEmitted}/20" +
                    (ability.Selected == PG07Ability.PioggiaDiFrecce ? " | Tieni Q: anteprima; rilascia: attiva" : ""));
                GUI.Label(new Rect(20, Screen.height - 86, Screen.width - 40, 26),
                    $"{ability.PassiveLabel} | ATTIVAZIONI {ability.PassiveTriggers} / ROLL {ability.PassiveRolls}");
            }
            if (loop.PG06Ability != null)
            {
                var ability = loop.PG06Ability;
                var vitamin = loop.Player.GetComponent<PG06Vitamin>();
                GUI.Label(new Rect(20, Screen.height - 60, Screen.width - 40, 26),
                    $"Q — {ability.Label} | CD {ability.CooldownRemaining:0.0} s | CARICHE {ability.Charges}");
                GUI.Label(new Rect(20, Screen.height - 86, Screen.width - 40, 26),
                    $"{ability.PassiveLabel} | VITAMINA C {(vitamin != null ? vitamin.Remaining : 0):0.0} s | ATK SPD {loop.Player.Actor.EffectiveStats.AttackSpeed:0.##} | DROP {ability.Drops}");
            }
            if (loop.PG05Ability != null)
            {
                var ability = loop.PG05Ability;
                var hidden = loop.Player.GetComponent<PG05Invisibility>();
                GUI.Label(new Rect(20, Screen.height - 60, Screen.width - 40, 26),
                    $"Q — {ability.Label} | CD {ability.CooldownRemaining:0.0} s | INVISIBILITA {(hidden != null ? hidden.Remaining : 0):0.0} s");
                GUI.Label(new Rect(20, Screen.height - 86, Screen.width - 40, 26),
                    $"{ability.PassiveLabel} | CICUTA {ability.CicutaHits}/15 | MOVE {loop.Player.Actor.MovementMetresPerSecond:0.##} m/s");
            }
            if (loop.PG04Ability != null)
            {
                var ability = loop.PG04Ability;
                GUI.Label(new Rect(20, Screen.height - 60, Screen.width - 40, 26),
                    $"Q — {ability.Label} | CD {ability.CooldownRemaining:0.0} s | DURATA {ability.ActiveRemaining:0.0} s | CARICHE {ability.Charges}" +
                    (ability.Selected == PG04Ability.PioggiaDiGranate ? " | Tieni Q: anteprima; rilascia: attiva" : ""));
                GUI.Label(new Rect(20, Screen.height - 86, Screen.width - 40, 26), $"{ability.PassiveLabel} | AUTOMATICA");
                GUI.Label(new Rect(20, Screen.height - 185, 600, 22), "TEST SLOT ITEM — carica/consuma senza lanciare ITEMS");
                GUI.enabled = loop.GameplayRunning;
                for (int i = 0; i < loop.PG04Items.SlotCount; i++)
                {
                    var items = loop.PG04Items;
                    float x = 20 + i * 230;
                    GUI.Label(new Rect(x, Screen.height - 161, 220, 22), $"SLOT {i + 1}: {items.Kind(i)} × {items.Count(i)}");
                    if (GUI.Button(new Rect(x, Screen.height - 135, 75, 24), "+GRANATA")) items.TryAdd(i, PrototypeItem.Granata);
                    if (GUI.Button(new Rect(x + 78, Screen.height - 135, 75, 24), "+MOLOTOV")) items.TryAdd(i, PrototypeItem.Molotov);
                    if (GUI.Button(new Rect(x + 156, Screen.height - 135, 70, 24), "Consuma 1")) items.TryConsume(i);
                }
                GUI.enabled = true;
            }
            GUI.Label(new Rect(20, Screen.height - 34, Screen.width - 40, 28), "Prototype: roster PG01–PG08 + ZOMB01; ABILITÀ BONUS e LEVEL UP attivi.");
            if (loop.State == LoopState.AreaComplete)
            {
                Vector3 view = loop.GameCamera.WorldToViewportPoint(loop.Exit.transform.position);
                bool visible = view.z > 0 && view.x >= 0 && view.x <= 1 && view.y >= 0 && view.y <= 1;
                GUI.Label(new Rect(20, 190, Screen.width - 40, 30), "AREA COMPLETATA — raggiungi l'USCITA verde (raggio 4 m)");
                if (!visible)
                {
                    Vector2 delta = loop.Exit.transform.position - loop.Player.transform.position;
                    float angle = Mathf.Atan2(-delta.y, delta.x) * Mathf.Rad2Deg;
                    var matrix = GUI.matrix;
                    var pivot = new Vector2(Screen.width / 2f, 230);
                    GUIUtility.RotateAroundPivot(angle, pivot);
                    GUI.Label(new Rect(pivot.x - 12, pivot.y - 12, 50, 30), "-->");
                    GUI.matrix = matrix;
                }
            }
            if (loop.Loading) GUI.Box(new Rect(30, 180, Screen.width - 60, 60), "Caricamento AREA / FIRST SPAWN off-screen...");
            if (loop.State == LoopState.Error) GUI.Box(new Rect(30, 180, Screen.width - 60, 80), loop.Failure);
            if (loop.State == LoopState.Defeat) GUI.Box(new Rect(30, 180, Screen.width - 60, 60), "SCONFITTA: PG DOWN, nessun PG attivo. Riprova test.");
            if (loop.State == LoopState.Finished) GUI.Box(new Rect(30, 180, Screen.width - 60, 60), "Due cicli completati. Fine del test tecnico; Riprova test per una nuova prova.");
            if (loop.BonusAbilities != null)
                GUI.Label(new Rect(20, 136, Screen.width - 40, 25), loop.BonusAbilities.Status());
            if (loop.Experience != null)
            {
                var exp = loop.Experience;
                GUI.Label(new Rect(20, 162, Screen.width - 40, 24), $"LVL {exp.Level} | EXP {exp.Experience}/{exp.NextThreshold} | Scelte pendenti {exp.PendingChoices}");
                if (exp.Choices != null)
                {
                    GUI.Box(new Rect(10, 205, Screen.width - 20, 140), "LEVEL UP — scegli un BONUS");
                    float bannerWidth = (Screen.width - 60) / 3f;
                    for (int i = 0; i < exp.Choices.Length; i++)
                        if (GUI.Button(new Rect(20 + i * (bannerWidth + 10), 245, bannerWidth, 80), loop.Bonuses.Label(exp.Choices[i])))
                        { exp.Choose(i); break; }
                    return;
                }
            }
            if (loop.State != LoopState.Bonus) return;
            GUI.Box(new Rect(10, 140, Screen.width - 20, 320), "BONUS FINE AREA — scegli un BONUS, poi conferma");
            float width = (Screen.width - 60) / 3f;
            for (int i = 0; i < 3; i++)
                if (GUI.Button(new Rect(20 + i * (width + 10), 185, width, 65),
                    (loop.Selected == i ? "[X] " : "") + AreaStatBonus.Labels[(int)loop.Choices[i]])) loop.SelectBonus(i);
            for (int i = 0; i < loop.AbilityChoices.Length; i++)
                if (GUI.Button(new Rect(20 + i * ((Screen.width - 50) / 2f + 10), 265, (Screen.width - 50) / 2f, 65),
                    (loop.Selected == i + 3 ? "[X] " : "") + loop.Bonuses.Label(loop.AbilityChoices[i]))) loop.SelectBonus(i + 3);
            GUI.enabled = loop.Selected >= 0;
            if (GUI.Button(new Rect(20, 350, Screen.width - 40, 60), "Conferma")) loop.ConfirmBonus();
            GUI.enabled = true;
        }
    }
}
