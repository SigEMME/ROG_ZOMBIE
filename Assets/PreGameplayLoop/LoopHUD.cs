using UnityEngine;

namespace RogZombie.PreGameplayLoop
{
    public sealed class LoopHUD : MonoBehaviour
    {
        private LoopSession loop;
        private void Awake() => loop = GetComponent<LoopSession>();

        private void DrawResurrection(LoopSession member)
        {
            if (member.Player == null || loop.GameCamera == null || member.Resurrection == null) return;
            var actor = member.Player.Actor;
            string text = actor.State == RogZombie.TestEngine.LifeState.Down ?
                $"DOWN {member.Resurrection.DownRemaining:0.0}s | Tieni F entro 2 m | {member.Resurrection.Progress:0.0}/5s" :
                actor.State == RogZombie.TestEngine.LifeState.Dead ? "MORTE — ritorno nella prossima AREA" :
                actor.ResurrectionProtectionRemaining > 0 ? $"INVULNERABILE {actor.ResurrectionProtectionRemaining:0.0}s" : null;
            if (text == null) return;
            Vector3 point = loop.GameCamera.WorldToScreenPoint(actor.transform.position);
            GUI.Box(new Rect(point.x - 190, Screen.height - point.y - 65, 380, 26), text);
        }
        private void OnGUI()
        {
            DrawResurrection(loop);
            if (loop.Companion != null) DrawResurrection(loop.Companion);
            var view = loop.Controlled != null ? loop.Controlled : loop;
            GUI.Box(new Rect(8, 8, Screen.width - 16, 94), "Pre gameplay loop prototype | WASD / Mouse / LMB / Q");
            GUI.Label(new Rect(20, 30, Screen.width - 160, 24), $"AREA {loop.AreaIndex + 1}/{loop.Definition.AreaTotals.Length} | {loop.State} | G {loop.Gold}");
            if (GUI.Button(new Rect(Screen.width - 140, 32, 115, 26), "Riprova test"))
                loop.RestartTest();
            if (view.Player != null)
                GUI.Label(new Rect(20, 58, Screen.width - 40, 26), $"{view.Player.Definition.PlayerId} HP {view.Player.Actor.CurrentHP:0.#}/{view.Player.Actor.Stats.HP:0.#} | CD REDUCTION {view.CdReduction}");
            if (loop.Spawns != null)
                GUI.Label(new Rect(20, 110, Screen.width - 40, 24), $"MOB {loop.Spawns.Alive}/{loop.Spawns.MaxSimultaneous} | Generati {loop.Spawns.TotalSpawned}/{loop.Settings.TotalMobs} | Morti {loop.Spawns.Killed}");
            if (view.Ability != null)
                GUI.Label(new Rect(20, Screen.height - 60, Screen.width - 40, 26),
                    $"Q — {view.Ability.Selected.ToString().ToUpperInvariant()} | CD {view.Ability.CooldownRemaining:0.0} s" +
                    (view.Ability.Selected == PG01Ability.Barriera ? " | Tieni Q: anteprima, rilascia: piazza" : ""));
            if (view.Passive != null && view.Player != null)
                GUI.Label(new Rect(20, Screen.height - 86, Screen.width - 40, 26),
                    $"{view.Passive.Label} | {(view.Passive.EffectActive ? "ATTIVA" : "INATTIVA")} | " +
                    $"ATK {view.Player.Actor.EffectiveStats.ATK:0.##} | DEF {view.Player.Actor.EffectiveStats.DEF - 100:0.##}%");
            if (view.PG02Ability != null)
                GUI.Label(new Rect(20, Screen.height - 60, Screen.width - 40, 26),
                    $"Q — {view.PG02Ability.Label} | CD {view.PG02Ability.CooldownRemaining:0.0} s | DURATA {view.PG02Ability.ActiveRemaining:0.0} s");
            if (view.PG02Passive != null && view.Player != null)
            {
                var passive = view.PG02Passive;
                var stats = view.Player.Actor.EffectiveStats;
                GUI.Label(new Rect(20, Screen.height - 110, Screen.width - 40, 26),
                    $"{passive.Label} | {(passive.EffectActive ? "ATTIVA" : "INATTIVA")} | KILL {passive.Kills}" +
                    (passive.Selected == PG02Passive.Passiva2 ? $" | RAGE {passive.KillsTowardRage}/15 — {passive.RageRemaining:0.0} s" : ""));
                GUI.Label(new Rect(20, Screen.height - 86, Screen.width - 40, 26),
                    $"ATK {stats.ATK:0.##} | ATK SPD {stats.AttackSpeed:0.##} ({stats.AttackSpeed / 100f:0.##}/s) | DEF {stats.DEF - 100:0.##}%");
            }
            if (view.PG03Ability != null && view.Player != null)
            {
                GUI.Label(new Rect(20, Screen.height - 60, Screen.width - 40, 26),
                    $"Q — {view.PG03Ability.Label} | CD {view.PG03Ability.CooldownRemaining:0.0} s | DURATA {view.PG03Ability.ActiveRemaining:0.0} s");
                GUI.Label(new Rect(20, Screen.height - 86, Screen.width - 40, 26),
                    $"{view.PG03Passive.Label} | AUTOMATICA | ATK {view.Player.Actor.EffectiveStats.ATK:0.##} | MOVE SPD {view.Player.Actor.Stats.MoveSpeed:0.##}");
            }
            if (view.PG08Ability != null)
            {
                var ability = view.PG08Ability; var passive = view.PG08Passive; var stats = view.Player.Actor.EffectiveStats;
                GUI.Label(new Rect(20, Screen.height - 60, Screen.width - 40, 26),
                    $"Q — {ability.Label} | CD {ability.CooldownRemaining:0.0} s | DURATA {ability.DurationRemaining:0.0} s");
                GUI.Label(new Rect(20, Screen.height - 86, Screen.width - 40, 26),
                    $"{passive.Label} | {(passive.EffectActive ? "ATTIVA" : "INATTIVA")} | " +
                    (passive.Selected == PG08Passive.Tenacia ? $"HIT {passive.Hits}/150 | BONUS DEF +{passive.TenacityBonus:0.0}%" : $"FUOCO {passive.ContinuousFire:0.0}/5 s") +
                    $" | ATK {stats.ATK:0.##} | DEF {stats.DEF - 100:0.##}% | MOVE SPD {stats.MoveSpeed:0.##}");
            }
            if (view.PG07Ability != null)
            {
                var ability = view.PG07Ability;
                GUI.Label(new Rect(20, Screen.height - 60, Screen.width - 40, 26),
                    $"Q — {ability.Label} | CD {ability.CooldownRemaining:0.0} s | FRECCE {ability.RainEmitted}/20" +
                    (ability.Selected == PG07Ability.PioggiaDiFrecce ? " | Tieni Q: anteprima; rilascia: attiva" : ""));
                GUI.Label(new Rect(20, Screen.height - 86, Screen.width - 40, 26),
                    $"{ability.PassiveLabel} | ATTIVAZIONI {ability.PassiveTriggers} / ROLL {ability.PassiveRolls}");
            }
            if (view.PG06Ability != null)
            {
                var ability = view.PG06Ability;
                var vitamin = view.Player.GetComponent<PG06Vitamin>();
                GUI.Label(new Rect(20, Screen.height - 60, Screen.width - 40, 26),
                    $"Q — {ability.Label} | CD {ability.CooldownRemaining:0.0} s | CARICHE {ability.Charges}");
                GUI.Label(new Rect(20, Screen.height - 86, Screen.width - 40, 26),
                    $"{ability.PassiveLabel} | VITAMINA C {(vitamin != null ? vitamin.Remaining : 0):0.0} s | ATK SPD {view.Player.Actor.EffectiveStats.AttackSpeed:0.##} | DROP {ability.Drops}");
            }
            if (view.PG05Ability != null)
            {
                var ability = view.PG05Ability;
                var hidden = view.Player.GetComponent<PG05Invisibility>();
                GUI.Label(new Rect(20, Screen.height - 60, Screen.width - 40, 26),
                    $"Q — {ability.Label} | CD {ability.CooldownRemaining:0.0} s | INVISIBILITA {(hidden != null ? hidden.Remaining : 0):0.0} s");
                GUI.Label(new Rect(20, Screen.height - 86, Screen.width - 40, 26),
                    $"{ability.PassiveLabel} | CICUTA {ability.CicutaHits}/15 | MOVE {view.Player.Actor.MovementMetresPerSecond:0.##} m/s");
            }
            if (view.PG04Ability != null)
            {
                var ability = view.PG04Ability;
                GUI.Label(new Rect(20, Screen.height - 60, Screen.width - 40, 26),
                    $"Q — {ability.Label} | CD {ability.CooldownRemaining:0.0} s | DURATA {ability.ActiveRemaining:0.0} s | CARICHE {ability.Charges}" +
                    (ability.Selected == PG04Ability.PioggiaDiGranate ? " | Tieni Q: anteprima; rilascia: attiva" : ""));
                GUI.Label(new Rect(20, Screen.height - 86, Screen.width - 40, 26), $"{ability.PassiveLabel} | AUTOMATICA");
                GUI.Label(new Rect(20, Screen.height - 185, 600, 22), "TEST SLOT ITEM — carica/consuma senza lanciare ITEMS");
                GUI.enabled = loop.GameplayRunning;
                for (int i = 0; i < view.PG04Items.SlotCount; i++)
                {
                    var items = view.PG04Items;
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
                Vector3 exitView = loop.GameCamera.WorldToViewportPoint(loop.Exit.transform.position);
                bool visible = exitView.z > 0 && exitView.x >= 0 && exitView.x <= 1 && exitView.y >= 0 && exitView.y <= 1;
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
            var choiceContext = loop.ChoiceContext;
            if (choiceContext.Experience != null)
            {
                var exp = choiceContext.Experience;
                GUI.Label(new Rect(20, 162, Screen.width - 40, 24), $"LVL {exp.Level} | EXP {exp.Experience}/{exp.NextThreshold} | Scelte pendenti {exp.PendingChoices}");
                if (exp.Choices != null)
                {
                    GUI.Box(new Rect(10, 205, Screen.width - 20, 140), "LEVEL UP — " + choiceContext.Player.Definition.PlayerId + " — scegli un BONUS");
                    float bannerWidth = (Screen.width - 60) / 3f;
                    for (int i = 0; i < exp.Choices.Length; i++)
                        if (GUI.Button(new Rect(20 + i * (bannerWidth + 10), 245, bannerWidth, 80), choiceContext.Bonuses.Label(exp.Choices[i])))
                        { exp.Choose(i); break; }
                    return;
                }
            }
            if (loop.Companion != null)
            {
                var companion = loop.Companion;
                var exp = companion.Experience;
                GUI.Label(new Rect(20, 186, Screen.width - 40, 24), $"PG IA 1 — {companion.Player.Definition.PlayerId} HP {companion.Player.Actor.CurrentHP:0.#}/{companion.Player.Actor.Stats.HP:0.#} | LVL {exp.Level} EXP {exp.Experience}/{exp.NextThreshold} | SPACE + 1: ABILITA | controllo: {view.Player.Definition.PlayerId}");
            }
            if (loop.State != LoopState.Bonus) return;
            GUI.Box(new Rect(10, 140, Screen.width - 20, 320), "BONUS FINE AREA — " + loop.RewardContext.Player.Definition.PlayerId + " — scegli un BONUS, poi conferma");
            float width = (Screen.width - 60) / 3f;
            for (int i = 0; i < 3; i++)
                if (GUI.Button(new Rect(20 + i * (width + 10), 185, width, 65),
                    (loop.Selected == i ? "[X] " : "") + AreaStatBonus.Labels[(int)loop.Choices[i]])) loop.SelectBonus(i);
            for (int i = 0; i < loop.AbilityChoices.Length; i++)
                if (GUI.Button(new Rect(20 + i * ((Screen.width - 50) / 2f + 10), 265, (Screen.width - 50) / 2f, 65),
                    (loop.Selected == i + 3 ? "[X] " : "") + loop.RewardContext.Bonuses.Label(loop.AbilityChoices[i]))) loop.SelectBonus(i + 3);
            GUI.enabled = loop.Selected >= 0;
            if (GUI.Button(new Rect(20, 350, Screen.width - 40, 60), "Conferma")) loop.ConfirmBonus();
            GUI.enabled = true;
        }
    }
}
