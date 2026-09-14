using System.Collections.Generic;
using UnityEngine;

namespace RogZombie.TestEngine
{
    // Presentation only: healing amounts are measured by Combatant after the HP cap.
    public sealed class HealingNumbers : MonoBehaviour
    {
        public Camera ViewCamera;
        private const float Duration = 1.2f;
        private struct Number
        {
            public Combatant Target;
            public float Amount, Started;
            public int Lane;
        }
        private readonly List<Number> numbers = new List<Number>();
        private GUIStyle style;
        private void OnEnable() => Combatant.HealingApplied += Show;
        private void OnDisable() { Combatant.HealingApplied -= Show; numbers.Clear(); }
        private void Show(Combatant target, float amount)
        {
            if (target == null || target.Faction != Faction.PG || amount <= 0) return;
            int lane = 0;
            foreach (var number in numbers) if (number.Target == target) lane++;
            if (numbers.Count >= 64) numbers.RemoveAt(0);
            numbers.Add(new Number { Target = target, Amount = amount, Started = Time.time, Lane = lane % 4 });
        }
        private void Update() => numbers.RemoveAll(number => number.Target == null || Time.time - number.Started >= Duration);
        private void OnGUI()
        {
            if (ViewCamera == null) return;
            if (style == null) style = new GUIStyle(GUI.skin.label)
            { alignment = TextAnchor.MiddleCenter, fontSize = 20, fontStyle = FontStyle.Bold };
            foreach (var number in numbers)
            {
                if (number.Target == null || !number.Target.gameObject.activeInHierarchy) continue;
                float age = Time.time - number.Started;
                Vector3 screen = ViewCamera.WorldToScreenPoint(number.Target.transform.position + Vector3.up * (.9f + age * .5f));
                if (screen.z <= 0) continue;
                float alpha = Mathf.Clamp01((Duration - age) / .4f);
                Rect rect = new Rect(screen.x - 70, Screen.height - screen.y - 20 - number.Lane * 23, 140, 32);
                string label = "+" + number.Amount.ToString("0.###") + " HP";
                style.normal.textColor = new Color(0, 0, 0, alpha);
                GUI.Label(new Rect(rect.x + 1, rect.y + 1, rect.width, rect.height), label, style);
                style.normal.textColor = new Color(.25f, 1, .4f, alpha);
                GUI.Label(rect, label, style);
            }
        }
    }
}
