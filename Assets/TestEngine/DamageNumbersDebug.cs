using System.Collections.Generic;
using UnityEngine;

namespace RogZombie.TestEngine
{
    public sealed class DamageNumbersDebug : MonoBehaviour
    {
        public TestAreaSettings Settings;
        public Camera ViewCamera;
        private struct Number { public Vector3 Position; public float Damage, Started; public Faction Target; }
        private readonly List<Number> numbers = new List<Number>();
        private GUIStyle style;
        private void OnEnable() => Combatant.DamageApplied += Show;
        private void OnDisable() { Combatant.DamageApplied -= Show; numbers.Clear(); }
        private void Show(Combatant target, float damage)
        {
            if (Settings == null || !Settings.ShowDamageNumbers) return;
            numbers.Add(new Number { Position = target.transform.position, Damage = damage, Started = Time.time, Target = target.Faction });
        }
        private void Update()
        {
            if (Settings == null) return;
            if (!Settings.ShowDamageNumbers) { numbers.Clear(); return; }
            numbers.RemoveAll(number => Time.time - number.Started >= Settings.DamageNumberSeconds);
        }
        private void OnGUI()
        {
            if (Settings == null || !Settings.ShowDamageNumbers || ViewCamera == null) return;
            if (style == null) style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 18, fontStyle = FontStyle.Bold };
            foreach (var number in numbers)
            {
                float age = Time.time - number.Started;
                Vector3 screen = ViewCamera.WorldToScreenPoint(number.Position + Vector3.up * (0.5f + age));
                if (screen.z <= 0f) continue;
                style.normal.textColor = number.Target == Faction.PG ? new Color(1f, 0.4f, 0.3f) : Color.yellow;
                GUI.Label(new Rect(screen.x - 55f, Screen.height - screen.y - 15f, 110f, 30f), number.Damage.ToString("0.###"), style);
            }
        }
    }
}
