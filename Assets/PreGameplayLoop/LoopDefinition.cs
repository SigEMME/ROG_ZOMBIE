using UnityEngine;
using RogZombie.TestEngine;

namespace RogZombie.PreGameplayLoop
{
    [CreateAssetMenu(menuName = "ROG ZOMBIE/Pre gameplay loop")]
    public sealed class LoopDefinition : ScriptableObject
    {
        public TestAreaSettings GeometryTemplate;
        public WeaponDefinition PG01Weapon;
        public MobDefinition ZOMB01;
        [Tooltip("GDD section 14: C1 A1 and A2 totals. ZOMB01-only test composition.")]
        public int[] AreaTotals = { 100, 120 };
        [Tooltip("Technical level placement; no final level design is implied.")]
        public Vector2 ExitPosition;

        public string Validate()
        {
            if (GeometryTemplate == null || PG01Weapon == null || PG01Weapon.PG == null || ZOMB01 == null)
                return "Assegnare geometria, arma PG01 e ZOMB01.";
            if (PG01Weapon.PG.PlayerId != "PG01" || ZOMB01.Kind != MobKind.ZOMB01)
                return "Questo slice richiede PG01 e ZOMB01.";
            if (AreaTotals == null || AreaTotals.Length != 2) return "Configurare esattamente due AREE test.";
            foreach (int total in AreaTotals)
                if (total <= 0 || total % 10 != 0) return "Totali positivi multipli di 10: FIRST SPAWN 30% intero.";
            var g = GeometryTemplate;
            if (g.ActorRadius <= 0 || g.CameraSize <= 0 || g.AreaSize.x <= 0 || g.AreaSize.y <= 0 || g.SearchAttemptsPerFrame < 1)
                return "Geometria o budget di ricerca non valido.";
            if (!g.EnableMobSeparation) return "Il test richiede collisione/separazione MOB attiva.";
            if (Mathf.Abs(ExitPosition.x) + 4 >= g.AreaSize.x / 2 || Mathf.Abs(ExitPosition.y) + 4 >= g.AreaSize.y / 2)
                return "Il TRIGGER di uscita deve essere interno all'AREA.";
            return null;
        }
    }
}
