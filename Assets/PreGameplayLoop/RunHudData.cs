using UnityEngine;
using RogZombie.TestEngine;

namespace RogZombie.PreGameplayLoop
{
    // Presentation only. Time.time freezes together with the RUN and its cooldowns.
    public sealed class HudPulse
    {
        private float until = float.NegativeInfinity;
        public bool Active => Time.time < until;
        public void Trigger() => until = Time.time + .5f;
    }

    public struct HudAbility
    {
        public string Label;
        public float Remaining, Progress;
        public HudAbility(string label, float remaining, float progress)
        { Label = label; Remaining = remaining; Progress = progress; }
    }

    public static class RunHudData
    {
        public static float ExperienceFraction(ExperienceProgression exp) =>
            exp != null && exp.NextThreshold > 0 ? Mathf.Clamp01((float)exp.Experience / exp.NextThreshold) : 0;
        public static int RemainingMobs(int total, int killed) => Mathf.Max(0, total - killed);

        public static HudAbility Ability(LoopSession m)
        {
            if (m.Ability != null) return new HudAbility(m.Ability.Selected.ToString().ToUpperInvariant(), m.Ability.CooldownRemaining, m.Ability.CooldownProgress);
            if (m.PG02Ability != null) return new HudAbility(m.PG02Ability.Label, m.PG02Ability.CooldownRemaining, m.PG02Ability.CooldownProgress);
            if (m.PG03Ability != null) return new HudAbility(m.PG03Ability.Label, m.PG03Ability.CooldownRemaining, m.PG03Ability.CooldownProgress);
            if (m.PG04Ability != null) return new HudAbility(m.PG04Ability.Label, m.PG04Ability.CooldownRemaining, m.PG04Ability.CooldownProgress);
            if (m.PG05Ability != null) return new HudAbility(m.PG05Ability.Label, m.PG05Ability.CooldownRemaining, m.PG05Ability.CooldownProgress);
            if (m.PG06Ability != null) return new HudAbility(m.PG06Ability.Label, m.PG06Ability.CooldownRemaining, m.PG06Ability.CooldownProgress);
            if (m.PG07Ability != null) return new HudAbility(m.PG07Ability.Label, m.PG07Ability.CooldownRemaining, m.PG07Ability.CooldownProgress);
            if (m.PG08Ability != null) return new HudAbility(m.PG08Ability.Label, m.PG08Ability.CooldownRemaining, m.PG08Ability.CooldownProgress);
            return new HudAbility("—", 0, 0);
        }

        public static bool Passive(LoopSession m, out string label)
        {
            label = "—";
            bool active = false;
            if (m.Passive != null) { label = m.Passive.Label; active = m.Passive.EffectActive; }
            else if (m.PG02Passive != null)
            { label = m.PG02Passive.Label; active = m.PG02Passive.Selected == PG02Passive.Passiva1 ? m.PG02Passive.HealPulse.Active : m.PG02Passive.RageActive; }
            else if (m.PG03Passive != null)
            { label = m.PG03Passive.Label; active = m.PG03Passive.Selected == PG03Passive.CalibroPerforante || m.PG03Passive.MarkActive || m.PG03Passive.MarkPulse.Active; }
            else if (m.PG04Ability != null)
            { label = m.PG04Ability.PassiveLabel; active = m.PG04Ability.Passive == PG04Passive.ScortaEsplosiva || m.PG04Ability.PyromaniaActive; }
            else if (m.PG05Ability != null)
            { label = m.PG05Ability.PassiveLabel; active = m.PG05Ability.Passive == PG05Passive.Ghosting ? m.PG05Ability.GhostActive : (m.PG05Ability.CicutaActive || m.PG05Ability.PoisonPulse.Active); }
            else if (m.PG06Ability != null)
            {
                label = m.PG06Ability.PassiveLabel;
                if (m.PG06Ability.Passive == PG06Passive.Elemosina) active = m.PG06Ability.DropPulse.Active;
                else foreach (var member in m.World.Members)
                {
                    if (member.Player == null) continue;
                    var vitamin = member.Player.GetComponent<PG06Vitamin>();
                    if (vitamin != null && vitamin.Source == m && vitamin.Remaining > 0) active = true;
                }
            }
            else if (m.PG07Ability != null) { label = m.PG07Ability.PassiveLabel; active = m.PG07Ability.PassivePulse.Active; }
            else if (m.PG08Passive != null) { label = m.PG08Passive.Label; active = m.PG08Passive.EffectActive; }
            return active && m.Player != null && m.Player.Actor.IsActive;
        }

        public static string ItemAsset(PrototypeItem item)
        {
            switch (item)
            {
                case PrototypeItem.Granata: return "granata";
                case PrototypeItem.Molotov: return "molotov";
                case PrototypeItem.Smoke: return "smoke";
                case PrototypeItem.Trappola: return "trappola";
                case PrototypeItem.PozioneCurativa: return "pozione-curativa";
                case PrototypeItem.BombaVelenosa: return "bomba-velenosa";
                case PrototypeItem.MinaElettrica: return "mina-elettrica";
                default: return null;
            }
        }

        // Each quarter is one edge: bottom, left, top, right, starting bottom-right.
        public static float HealthEdge(float hpFraction, int edge) => Mathf.Clamp01(4 * Mathf.Clamp01(hpFraction) - (3 - edge));
    }
}
