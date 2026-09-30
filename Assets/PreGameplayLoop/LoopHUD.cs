using UnityEngine;

namespace RogZombie.PreGameplayLoop
{
    public sealed partial class LoopHUD : MonoBehaviour
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
            if (loop.PauseMenuOpen) return;
            if (loop.GetComponent<RogZombie.BossTest.BossTestScene>()?.Presenting == true) return;
            foreach (var member in loop.Members) DrawResurrection(member);
            var view = loop.Controlled != null ? loop.Controlled : loop;
            DrawRunHud(view);
            if (loop.State == LoopState.AreaComplete)
            {
                Vector3 exitView = loop.GameCamera.WorldToViewportPoint(loop.Exit.transform.position);
                bool visible = exitView.z > 0 && exitView.x >= 0 && exitView.x <= 1 && exitView.y >= 0 && exitView.y <= 1;
                GUI.Label(new Rect(20, 270, Screen.width - 40, 30), "AREA COMPLETATA — raggiungi l'USCITA verde (raggio 4 m)");
                if (!visible)
                {
                    Vector2 delta = loop.Exit.transform.position - loop.Player.transform.position;
                    float angle = Mathf.Atan2(-delta.y, delta.x) * Mathf.Rad2Deg;
                    var matrix = GUI.matrix;
                    var pivot = new Vector2(Screen.width / 2f, 310);
                    GUIUtility.RotateAroundPivot(angle, pivot);
                    GUI.Label(new Rect(pivot.x - 12, pivot.y - 12, 50, 30), "-->");
                    GUI.matrix = matrix;
                }
            }
            if (loop.Loading)
            {
                var bossTest = loop.GetComponent<RogZombie.BossTest.BossTestScene>();
                string message = bossTest != null && bossTest.Presenting ? bossTest.PhaseLabel : "Caricamento AREA / FIRST SPAWN off-screen...";
                GUI.Box(new Rect(30, 180, Screen.width - 60, 60), message);
            }
            if (loop.State == LoopState.Error) GUI.Box(new Rect(30, 180, Screen.width - 60, 80), loop.Failure);
            if (loop.State == LoopState.Defeat) GUI.Box(new Rect(30, 180, Screen.width - 60, 60), "SCONFITTA: PG DOWN, nessun PG attivo. Riprova test.");
            if (loop.State == LoopState.Finished) GUI.Box(new Rect(30, 180, Screen.width - 60, 60), "RUN completata. Riprova test per una nuova prova.");
            if (loop.State == LoopState.Defeat || loop.State == LoopState.Finished || loop.State == LoopState.Error)
                if (GUI.Button(new Rect(Screen.width / 2f - 70, 270, 140, 30), "Riprova test")) loop.RestartTest();
            var choiceContext = loop.ChoiceContext;
            if (choiceContext.Experience != null)
            {
                var exp = choiceContext.Experience;
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
