using System.Collections.Generic;
using UnityEngine;

namespace RogZombie.PreGameplayLoop
{
    public sealed partial class LoopHUD
    {
        private readonly Dictionary<PrototypeItem, Texture2D> itemIcons = new Dictionary<PrototypeItem, Texture2D>();
        private GUIStyle hudText;
        private string hoverText;
        public static bool Displaying { get; private set; }
        private void OnEnable() => Displaying = true;
        private void OnDisable() => Displaying = false;
        private static readonly Color Panel = new Color(.035f, .045f, .055f, .92f);
        private static readonly Color Gold = new Color(.95f, .72f, .22f);
        private static readonly Color BonusGreen = new Color(.4f, .85f, .3f);

        private static void Fill(Rect rect, Color color)
        {
            if (rect.width <= 0 || rect.height <= 0) return;
            var previous = GUI.color;
            GUI.color = color; GUI.DrawTexture(rect, Texture2D.whiteTexture); GUI.color = previous;
        }
        private static void Frame(Rect rect, Color color, float thickness = 1.5f)
        {
            Fill(new Rect(rect.x, rect.y, rect.width, thickness), color);
            Fill(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
            Fill(new Rect(rect.x, rect.y, thickness, rect.height), color);
            Fill(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
        }
        private void Text(Rect rect, string text, int size, Color color)
        {
            hudText.fontSize = size; hudText.normal.textColor = color;
            GUI.Label(rect, text, hudText);
        }
        private void Hint(Rect rect, string text)
        { if (rect.Contains(Event.current.mousePosition)) hoverText = text; }

        private static string Sigla(string label)
        {
            switch (label)
            {
                case "PESTONE": return "PES";
                case "BARRIERA": return "BAR";
                case "PASSIVA 1 — DEF +15%": return "DEF";
                case "PASSIVA 2 — ATK +15%": return "ATK";
            }
            string result = "";
            foreach (var word in label.Split(' ', '—', '-', '_'))
                if (word.Length > 0) result += word[0];
            return result.Length > 1 ? result : label.Substring(0, Mathf.Min(3, label.Length));
        }
        private void AbilityIcon(Rect rect, HudAbility ability, Color color, string key)
        {
            Fill(rect, Panel);
            var inset = new Rect(rect.x + 2, rect.y + 2, rect.width - 4, rect.height - 4);
            Fill(inset, new Color(.25f, .25f, .25f));
            Fill(new Rect(inset.x, inset.y, inset.width * ability.Progress, inset.height), color * .65f + new Color(0, 0, 0, .35f));
            Frame(rect, ability.Progress >= 1 ? color : Color.gray);
            Text(rect, (string.IsNullOrEmpty(key) ? "" : key + " · ") + Sigla(ability.Label), rect.height < 19 ? 9 : 11, Color.white);
            Hint(rect, ability.Label + (ability.Remaining > 0 ? $" — CD {ability.Remaining:0.0}s" : " — PRONTA") + (key.Length > 0 ? "\n" + key : ""));
        }
        private void Portrait(Rect rect, LoopSession member, bool main)
        {
            var actor = member.Player.Actor;
            Fill(rect, Panel);
            var inner = new Rect(rect.x + 6, rect.y + 6, rect.width - 12, rect.height - 12);
            Fill(inner, actor.IsActive ? new Color(.16f, .23f, .29f) : new Color(.2f, .2f, .2f));
            Text(new Rect(inner.x, inner.y, inner.width, inner.height * .65f), member.Player.Definition.PlayerId, main ? 20 : 13, Color.white);
            Text(new Rect(inner.x, inner.y + inner.height * .62f, inner.width, inner.height * .38f),
                actor.IsActive ? "LV " + member.Experience.Level : actor.State.ToString().ToUpperInvariant(), main ? 11 : 8, actor.IsActive ? Color.gray : Color.red);
            Frame(rect, new Color(.15f, .16f, .17f), 4);
            float fraction = actor.HealthFraction;
            var hpColor = new Color(.25f, .95f, .38f);
            float bottom = RunHudData.HealthEdge(fraction, 0), left = RunHudData.HealthEdge(fraction, 1);
            float top = RunHudData.HealthEdge(fraction, 2), right = RunHudData.HealthEdge(fraction, 3);
            Fill(new Rect(rect.x, rect.yMax - 4, rect.width * bottom, 4), hpColor);
            Fill(new Rect(rect.x, rect.y, 4, rect.height * left), hpColor);
            Fill(new Rect(rect.xMax - rect.width * top, rect.y, rect.width * top, 4), hpColor);
            Fill(new Rect(rect.xMax - 4, rect.yMax - rect.height * right, 4, rect.height * right), hpColor);
            Hint(rect, $"{member.Player.Definition.PlayerId} — HP {actor.CurrentHP:0.#}/{actor.Stats.HP:0.#}\nLV {member.Experience.Level} — EXP {member.Experience.Experience}/{member.Experience.NextThreshold}");
        }
        private void DrawRunHud(LoopSession view)
        {
            if (view.Player == null) return;
            if (hudText == null) hudText = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, wordWrap = false, padding = new RectOffset() };
            var oldMatrix = GUI.matrix;
            float scale = Mathf.Min(Screen.width / 1152f, Screen.height / 648f);
            if (scale <= 0) return;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1));
            float w = Screen.width / scale, h = Screen.height / scale;
            hoverText = null;
            var inactive = new Rect(11, 10, 84, 33);
            Fill(inactive, Panel); Frame(inactive, new Color(.17f, .32f, .2f));
            Hint(inactive, "Slot riservato — inattivo");
            var mobRect = new Rect(11, 58, 84, 44);
            Fill(mobRect, Panel); Frame(mobRect, new Color(.6f, .4f, .85f));
            int total = loop.Settings != null ? loop.Settings.TotalMobs : 0;
            int remaining = RunHudData.RemainingMobs(total, loop.Spawns != null ? loop.Spawns.Killed : 0);
            Text(mobRect, "MOB " + remaining, 15, Color.white);
            Hint(mobRect, $"MOB rimanenti: {remaining}/{total}\nComprende i MOB ancora da generare.");
            var goldRect = new Rect(w - 77, 8, 67, 32);
            Fill(goldRect, Panel); Frame(goldRect, new Color(.6f, .4f, .25f));
            Text(goldRect, loop.Gold + " G", 13, Gold); Hint(goldRect, "G della RUN: " + loop.Gold);
            var expRect = new Rect(4, h - 194, 14, 188);
            Fill(expRect, Panel); Frame(expRect, Color.cyan, 1);
            float expHeight = (expRect.height - 4) * RunHudData.ExperienceFraction(view.Experience);
            Fill(new Rect(expRect.x + 2, expRect.yMax - 2 - expHeight, expRect.width - 4, expHeight), new Color(.1f, .8f, .95f));
            Hint(expRect, $"LV {view.Experience.Level} — EXP {view.Experience.Experience}/{view.Experience.NextThreshold}");
            Portrait(new Rect(23, h - 80, 74, 74), view, true);
            AbilityIcon(new Rect(23, h - 106, 74, 21), RunHudData.Ability(view), Gold, "Q");
            bool passiveActive = RunHudData.Passive(view, out string passiveName);
            var passiveRect = new Rect(24, h - 127, 72, 17);
            Fill(passiveRect, passiveActive ? new Color(.8f, .18f, .18f) : Panel);
            Frame(passiveRect, passiveActive ? new Color(1, .55f, .45f) : new Color(.4f, .18f, .18f));
            Text(passiveRect, Sigla(passiveName), 10, passiveActive ? Color.white : Color.gray);
            Hint(passiveRect, passiveName + (passiveActive ? " — ATTIVA" : " — INATTIVA"));
            for (int i = 0; i < 3; i++)
            {
                var rect = new Rect(25, h - 194 + i * 22, 60, 18);
                if (view.Bonuses != null && i < view.Bonuses.Owned.Count)
                {
                    var definition = view.Bonuses.Catalog.Bonuses[view.Bonuses.Owned[i].DefinitionIndex];
                    AbilityIcon(rect, new HudAbility(definition.Name, view.BonusAbilities.Cooldown(definition.Id), view.BonusAbilities.CooldownProgress(definition.Id)), BonusGreen, "");
                }
                else { Fill(rect, Panel); Frame(rect, BonusGreen * .5f); Text(rect, "—", 10, Color.gray); Hint(rect, "SLOT BONUS " + (i + 1) + " — VUOTO"); }
            }
            int companionIndex = 0;
            foreach (var member in loop.Members)
            {
                if (member == view || member.Player == null) continue;
                float x = 108 + companionIndex++ * 60;
                Portrait(new Rect(x, h - 54, 48, 48), member, false);
                AbilityIcon(new Rect(x, h - 74, 48, 15), RunHudData.Ability(member), new Color(.35f, .55f, 1), "");
                Hint(new Rect(x, h - 74, 48, 15), RunHudData.Ability(member).Label + (member.PartySlot > 0 ? $"\nSPACE + {member.PartySlot}" : "\nPG PLAYER"));
            }
            for (int i = 0; i < 4; i++)
            {
                var rect = new Rect(w - 168 + i * 39, h - 49, 35, 35);
                Fill(rect, Panel); Frame(rect, new Color(.85f, .45f, .7f));
                var slots = view.PG04Items;
                var kind = slots != null && i < slots.SlotCount ? slots.Kind(i) : PrototypeItem.None;
                int count = slots != null && i < slots.SlotCount ? slots.Count(i) : 0;
                if (kind != PrototypeItem.None)
                {
                    if (!itemIcons.TryGetValue(kind, out var icon))
                    { icon = Resources.Load<Texture2D>("MerchantItems/" + RunHudData.ItemAsset(kind)); itemIcons[kind] = icon; }
                    if (icon != null) GUI.DrawTexture(new Rect(rect.x + 2, rect.y + 2, 31, 31), icon, ScaleMode.ScaleToFit, true);
                }
                Text(new Rect(rect.x, rect.y - 13, rect.width, 12), (i + 1).ToString(), 10, Color.white);
                if (count > 1) { Fill(new Rect(rect.xMax - 18, rect.yMax - 13, 18, 13), Panel); Text(new Rect(rect.xMax - 18, rect.yMax - 13, 18, 13), "×" + count, 9, Color.white); }
                Hint(rect, kind == PrototypeItem.None ? "SLOT ITEM " + (i + 1) + " — VUOTO" : ItemRuntime.Label(kind) + " ×" + count + "\nTieni " + (i + 1) + ": anteprima; rilascia: usa");
            }
            if (hoverText != null)
            {
                var mouse = Event.current.mousePosition;
                var tip = new Rect(Mathf.Clamp(mouse.x + 14, 2, w - 310), Mathf.Clamp(mouse.y - 64, 2, h - 62), 308, 60);
                Fill(tip, new Color(.03f, .04f, .05f, .97f)); Frame(tip, new Color(.4f, .45f, .5f));
                hudText.wordWrap = true; Text(tip, hoverText, 12, Color.white); hudText.wordWrap = false;
            }
            GUI.matrix = oldMatrix;
        }
    }
}
