using UnityEditor;
using UnityEngine;
using RogZombie.TestEngine;

namespace RogZombie.EditorTests
{
    [CustomEditor(typeof(TestEngineBootstrap))]
    public sealed class TestTuningInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var world = (TestEngineBootstrap)target;
            if (world.Settings == null) return;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Tuning prossimo test", EditorStyles.boldLabel);
            var config = new SerializedObject(world.Settings);
            config.Update();
            foreach (string field in new[] { "PlayerMoveSpeedBase", "ForceTestChestNearPlayer", "ShowDamageNumbers", "ShowSpawnCounters",
                "FirstSpawnPercent", "TotalMobs", "EnableMobSeparation", "MobSpacing", "MobSeparationSpeed" })
                EditorGUILayout.PropertyField(config.FindProperty(field));
            config.ApplyModifiedProperties();
            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.IntField("MAX MOB ACTIVE", world.Spawns != null ? world.Spawns.MaxSimultaneous :
                    Mathf.CeilToInt(world.Settings.TotalMobs * world.Settings.FirstSpawnPercent / 100f));
            EditorGUILayout.HelpBox("Movimento globale, damage numbers e separazione: live. Popolazione/CHEST: alla prossima Riprova. ATK SPD del PG attivo: solo runtime; altri PG: asset per la prossima selezione.", MessageType.Info);
            foreach (var weapon in world.Settings.Weapons)
            {
                if (weapon == null || weapon.PG == null) continue;
                bool live = Application.isPlaying && world.Player != null && world.Player.Definition == weapon.PG;
                var source = new SerializedObject(live ? (Object)world.Player.Actor : weapon.PG);
                source.Update();
                EditorGUILayout.PropertyField(source.FindProperty(live ? "stats.AttackSpeed" : "baseStats.attackSpeed"),
                    new GUIContent(weapon.PG.PlayerId + " ATK SPD" + (live ? " (runtime)" : "")));
                source.ApplyModifiedProperties();
                if (weapon.Kind == BaseAttackKind.HitscanArea)
                {
                    var data = new SerializedObject(weapon);
                    data.Update();
                    EditorGUILayout.PropertyField(data.FindProperty("GrenadeHitDelay"), new GUIContent("PG04 GRENADE HIT DELAY (s)"));
                    data.ApplyModifiedProperties();
                }
            }
            foreach (var mob in world.Settings.Mobs)
            {
                if (mob == null || mob.Kind != MobKind.ZOMB04) continue;
                var data = new SerializedObject(mob);
                data.Update();
                EditorGUILayout.PropertyField(data.FindProperty("ProjectileSpeed"), new GUIContent("ZOMB04 PROJECTILE SPD (m/s)"));
                data.ApplyModifiedProperties();
            }
        }
    }
}
