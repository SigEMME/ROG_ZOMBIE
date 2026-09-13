using UnityEngine;
namespace RogZombie.PreGameplayLoop
{
    public sealed class RunPreparationUI : MonoBehaviour
    {
        private HubPrototype hub;
        private Vector2 descriptionScroll;
        private GUIStyle text, centre;
        private Texture2D circle;
        private readonly Color red = new Color(.85f, .08f, .15f), green = new Color(.12f, .65f, .3f),
            blue = new Color(.25f, .25f, .73f), yellow = new Color(.85f, .75f, 0), pink = new Color(.86f, .38f, .64f),
            purple = new Color(.6f, .25f, .65f), orange = new Color(1, .48f, .12f);
        private void Awake() => hub = GetComponent<HubPrototype>();
        private void Styles()
        {
            if (text != null) return;
            text = new GUIStyle(GUI.skin.label) { fontSize = 20, wordWrap = true, padding = new RectOffset(10, 10, 8, 8) };
            text.normal.textColor = text.hover.textColor = text.active.textColor = text.focused.textColor = Color.black;
            text.onNormal.textColor = text.onHover.textColor = text.onActive.textColor = text.onFocused.textColor = Color.black;
            centre = new GUIStyle(text) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            circle = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            for (int y = 0; y < 128; y++) for (int x = 0; x < 128; x++)
            {
                float radius = Vector2.Distance(new Vector2(x, y), new Vector2(63.5f, 63.5f));
                circle.SetPixel(x, y, radius <= 63 && radius >= 57 ? Color.white : Color.clear);
            }
            circle.Apply();
        }
        private static void Fill(Rect rect, Color color)
        { var old = GUI.color; GUI.color = color; GUI.DrawTexture(rect, Texture2D.whiteTexture); GUI.color = old; }
        private static void Outline(Rect rect, Color color)
        {
            Fill(new Rect(rect.x, rect.y, rect.width, 5), color); Fill(new Rect(rect.x, rect.yMax - 5, rect.width, 5), color);
            Fill(new Rect(rect.x, rect.y, 5, rect.height), color); Fill(new Rect(rect.xMax - 5, rect.y, 5, rect.height), color);
        }
        private bool BoxButton(Rect rect, string label, Color color, bool enabled = true)
        {
            Fill(rect, enabled ? new Color(.97f, .97f, .97f) : new Color(.84f, .84f, .84f));
            Outline(rect, enabled ? color : Color.gray);
            bool previous = GUI.enabled; GUI.enabled = previous && enabled;
            bool clicked = GUI.Button(rect, GUIContent.none, GUIStyle.none);
            GUI.enabled = previous;
            GUI.Label(new Rect(rect.x + 6, rect.y + 6, rect.width - 12, rect.height - 12), label, centre);
            return clicked;
        }
        private void OnGUI()
        {
            if (hub.Selection == null) return;
            Styles();
            var previous = GUI.matrix;
            float scale = Mathf.Min(Screen.width / 1536f, Screen.height / 864f);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1536 * scale) / 2, (Screen.height - 864 * scale) / 2), Quaternion.identity, Vector3.one * scale);
            if (hub.Selection.Page == PreparationPage.Run)
            {
                if (hub.Run != null && (hub.Run.State == LoopState.Finished || hub.Run.State == LoopState.Defeat))
                    if (BoxButton(new Rect(560, 450, 420, 75), "TORNA ALL'HUB", purple)) hub.ReturnFromRun();
            }
            else
            {
                Fill(new Rect(0, 0, 1536, 864), Color.white); Outline(new Rect(0, 0, 1536, 864), Color.black);
                if (hub.Failure != null) GUI.Label(new Rect(60, 30, 1400, 60), hub.Failure, text);
                else switch (hub.Selection.Page)
                {
                    case PreparationPage.Hub: DrawHub(); break;
                    case PreparationPage.Preparation: DrawPreparation(); break;
                    case PreparationPage.Selection: DrawSelection(); break;
                }
            }
            GUI.matrix = previous;
        }
        private void DrawHub()
        {
            GUI.Label(new Rect(50, 30, 1436, 60), "HUB DI PROVA", centre);
            GUI.Label(new Rect(50, 100, 1436, 65), "WASD: muoviti nell'HUB  |  Raggiungi EXIT e premi F per preparare la RUN", centre);
            Rect floor = new Rect(220, 200, 1080, 540); Fill(floor, new Color(.94f, .94f, .94f)); Outline(floor, Color.gray);
            Vector2 origin = floor.center;
            Vector2 exit = origin + new Vector2(hub.ExitPosition.x * 60, -hub.ExitPosition.y * 60);
            Outline(new Rect(exit.x - 60, exit.y - 60, 120, 120), green);
            GUI.Label(new Rect(exit.x - 60, exit.y - 60, 120, 120), "EXIT\n[F]", centre);
            Vector2 position = origin + new Vector2(hub.HubPosition.x * 60, -hub.HubPosition.y * 60);
            Fill(new Rect(position.x - 18, position.y - 18, 36, 36), blue);
            GUI.Label(new Rect(240, 760, 1040, 50), hub.AtExit ? "Premi F per PREPARAZIONE RUN" : "Un solo PG nella RUN · ROSTER interamente sbloccato", centre);
        }
        private string PGLabel => hub.Selection.Player < 0 ? "SELEZIONA PG" : "PG" + (hub.Selection.Player + 1).ToString("00");
        private string OptionLabel(bool passive)
        {
            int option = passive ? hub.Selection.Passive : hub.Selection.Ability;
            return hub.Selection.Player < 0 || option < 0 ? (passive ? "PASSIVA" : "ABILITÀ") : hub.Text.Characters[hub.Selection.Player].Options[(passive ? 2 : 0) + option].Name;
        }
        private void DrawPreparation()
        {
            GUI.Label(new Rect(470, 22, 600, 50), "PREPARAZIONE RUN", centre);
            for (int i = 0; i < 4; i++)
            {
                float x = 126 + i * 360;
                Outline(new Rect(x, 98, 190, 36), blue); GUI.Label(new Rect(x, 98, 190, 36), i == 0 ? "PLAYER 1" : "POSTO " + (i + 1), centre);
                string label = i == 0 ? PGLabel + (hub.Selection.Player >= 0 ? "\n" + hub.Text.Characters[hub.Selection.Player].Name : "") : "BLOCCATO";
                if (BoxButton(new Rect(x, 143, 190, 315), label, red, i == 0)) hub.Selection.OpenBanner(i);
                if (i != 0) continue;
                Outline(new Rect(x, 477, 40, 42), green); GUI.Label(new Rect(x + 45, 477, 160, 45), "ITEM VUOTO", text);
                DrawOptionCircle(new Rect(x, 537, 82, 82), "Q", yellow, false);
                DrawOptionCircle(new Rect(x + 108, 537, 82, 82), "P", pink, false);
                GUI.Label(new Rect(x - 50, 622, 295, 72), OptionLabel(false) + "\n" + OptionLabel(true), centre);
                GUI.Label(new Rect(x - 55, 690, 305, 40), hub.Selection.Confirmed ? "CONFERMATO" : "DA CONFERMARE", centre);
            }
            if (BoxButton(new Rect(506, 735, 398, 85), "CONFERMA PREPARAZIONE\nAVVIA RUN", Color.gray, hub.Selection.CanStart)) hub.BeginRun();
            if (BoxButton(new Rect(40, 800, 170, 44), "INDIETRO", purple)) hub.Selection.Back();
        }
        private bool DrawOptionCircle(Rect rect, string label, Color color, bool selected, bool enabled = false)
        {
            if (selected) Fill(new Rect(rect.x + 20, rect.y + 20, rect.width - 40, rect.height - 40), new Color(1, .96f, .75f));
            var previous = GUI.color; GUI.color = color; GUI.DrawTexture(rect, circle); GUI.color = previous;
            GUI.Label(new Rect(rect.x + 13, rect.y + 15, rect.width - 26, rect.height - 30), label, new GUIStyle(centre) { fontSize = 14 });
            return enabled && GUI.Button(rect, GUIContent.none, GUIStyle.none);
        }
        private void DrawSelection()
        {
            var model = hub.Selection;
            Outline(new Rect(16, 12, 810, 450), green); Outline(new Rect(16, 12, 278, 450), green);
            Outline(new Rect(948, 12, 578, 835), new Color(.6f, .83f, .87f));
            Outline(new Rect(975, 36, 530, 323), blue);
            GUI.Label(new Rect(990, 45, 500, 42), "STATS", centre);
            if (model.Player >= 0)
            {
                var character = hub.Text.Characters[model.Player];
                GUI.Label(new Rect(30, 25, 250, 420), character.Description, text);
                // Character art is intentionally a placeholder until dedicated PG sprites are supplied.
                GUI.Label(new Rect(310, 35, 500, 80), character.Name, centre);
                Fill(new Rect(488, 150, 140, 165), new Color(.8f, .82f, .85f));
                GUI.Label(new Rect(340, 335, 440, 85), PGLabel + "\nANTEPRIMA PROVVISORIA", centre);
                var values = RunPreparationSelection.Weapon(hub.Definition, model.Player).PG.BaseStats;
                GUI.Label(new Rect(995, 92, 495, 255), $"HP   {values.HP:0.##}\nATK   {values.ATK:0.##}\nDEF   {values.DefPercent:+0.##;-0.##;0}%\nMOVE SPD   {values.MoveSpeed:0.##}\nATK SPD   {values.AttackSpeed:0.##}\nCD REDUCTION   {values.CdReduction:0.##}", text);
            }
            else GUI.Label(new Rect(50, 100, 710, 150), "Scegli un PG dal ROSTER", centre);
            for (int i = 0; i < 8; i++)
            {
                Rect rect = new Rect(35 + (i % 4) * 154, 492 + (i / 4) * 152, 100, 120);
                if (BoxButton(rect, "PG" + (i + 1).ToString("00"), model.Player == i ? green : red))
                { model.SelectPlayer(i); descriptionScroll = Vector2.zero; }
            }
            for (int i = 0; i < 4; i++)
            {
                bool selected = i < 2 ? model.Ability == i : model.Passive == i - 2;
                string label = model.Player < 0 ? (i < 2 ? "ABILITÀ" : "PASSIVA") : hub.Text.Characters[model.Player].Options[i].Name;
                if (DrawOptionCircle(new Rect(963 + i * 139, 395, 132, 132), label, i < 2 ? yellow : pink, selected, model.Player >= 0))
                { model.SelectOption(i >= 2, i % 2); descriptionScroll = Vector2.zero; }
            }
            Outline(new Rect(985, 568, 515, 250), orange);
            string description = model.Player >= 0 && model.DescriptionIndex >= 0 ? hub.Text.Characters[model.Player].Options[model.DescriptionIndex].Name + "\n\n" + hub.Text.Characters[model.Player].Options[model.DescriptionIndex].Description : "Seleziona un’ABILITÀ o una PASSIVA per leggerne la descrizione.";
            var contentStyle = new GUIStyle(text) { fontSize = 18 };
            float height = Mathf.Max(225, contentStyle.CalcHeight(new GUIContent(description), 475));
            descriptionScroll = GUI.BeginScrollView(new Rect(993, 576, 499, 234), descriptionScroll, new Rect(0, 0, 475, height));
            GUI.Label(new Rect(0, 0, 475, height), description, contentStyle); GUI.EndScrollView();
            if (BoxButton(new Rect(660, 690, 220, 120), "CONFERMA", Color.gray, model.Complete)) model.ConfirmSelection();
            if (BoxButton(new Rect(48, 804, 180, 42), "INDIETRO", purple)) model.Back();
        }
        private void OnDestroy() { if (circle != null) Destroy(circle); }
    }
}
