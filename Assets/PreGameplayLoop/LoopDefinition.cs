using UnityEngine;
using RogZombie.TestEngine;

namespace RogZombie.PreGameplayLoop
{
    public enum LoopPlayer { PG01, PG02, PG03, PG04, PG05, PG06, PG07, PG08 }
    [System.Serializable]
    public sealed class AreaMobDistribution
    {
        public float[] Percentages = { 100, 0, 0, 0, 0 };
    }
    [System.Serializable]
    public sealed class AreaLayoutOverride
    {
        [Min(1)] public int AreaNumber = 3;
        public TestAreaSettings Geometry;
        public Vector2 ExitPosition;
    }
    [System.Serializable]
    public sealed class CompanionSelection
    {
        public bool Enabled;
        public LoopPlayer Player = LoopPlayer.PG03;
        [Range(0, 1)] public int Ability;
        [Range(0, 1)] public int Passive;
    }
    [CreateAssetMenu(menuName = "ROG ZOMBIE/Pre gameplay loop")]
    public sealed class LoopDefinition : ScriptableObject
    {
        [Header("Grafica TOP-DOWN provvisoria")]
        public Shader EnvironmentShader;
        [Header("ITEMS di prova")]
        public PrototypeItem[] StartingItems = new PrototypeItem[4];
        public int[] StartingItemCounts = new int[4];
        [Header("RUN")]
        public LoopPlayer SelectedPlayer = LoopPlayer.PG01;

        [Header("PG IA 1") ]
        public bool EnableCompanion;
        public LoopPlayer CompanionPlayer = LoopPlayer.PG02;
        [Range(0, 1)] public int CompanionAbility;
        [Range(0, 1)] public int CompanionPassive;
        [Header("PG IA 2 e 3")]
        public CompanionSelection[] AdditionalCompanions = { new CompanionSelection(), new CompanionSelection { Player = LoopPlayer.PG04 } };
        public CompanionSelection CompanionAt(int slot) => slot == 1
            ? new CompanionSelection { Enabled = EnableCompanion, Player = CompanionPlayer, Ability = CompanionAbility, Passive = CompanionPassive }
            : AdditionalCompanions != null && slot >= 2 && slot <= 3 && AdditionalCompanions.Length > slot - 2 ? AdditionalCompanions[slot - 2] : null;

        [Header("PG01")]
        public WeaponDefinition PG01Weapon;
        public PG01AbilityCatalog PG01Abilities;
        [Tooltip("Choose exactly one PG01 ability before RUN. Changes apply on Riprova test.")]
        public PG01Ability SelectedAbility = PG01Ability.Pestone;
        [Tooltip("Choose exactly one automatic PG01 passive before RUN. Changes apply on Riprova test.")]
        public PG01Passive SelectedPassive = PG01Passive.Passiva1;

        [Header("PG02")]
        public WeaponDefinition PG02Weapon;
        public PG02AbilityCatalog PG02Abilities;
        public PG02Ability SelectedPG02Ability = PG02Ability.FuocoRapido;
        public PG02Passive SelectedPG02Passive = PG02Passive.Passiva1;

        [Header("PG03")]
        public WeaponDefinition PG03Weapon;
        public PG03AbilityCatalog PG03Abilities;
        public PG03Ability SelectedPG03Ability = PG03Ability.ColpoLaser;
        public PG03Passive SelectedPG03Passive = PG03Passive.CalibroPerforante;

        [Header("PG04")]
        public WeaponDefinition PG04Weapon;
        public PG04AbilityCatalog PG04Abilities;
        public PG04Ability SelectedPG04Ability = PG04Ability.PioggiaDiGranate;
        public PG04Passive SelectedPG04Passive = PG04Passive.ScortaEsplosiva;
        [Range(1, 4), Tooltip("Technical item slot configuration, default one slot; no merchant simulation.")]
        public int PG04TestItemSlots = 1;

        [Header("PG05")]
        public WeaponDefinition PG05Weapon;
        public PG05AbilityCatalog PG05Abilities;
        public PG05Ability SelectedPG05Ability;
        public PG05Passive SelectedPG05Passive;

        [Header("PG06")]
        public WeaponDefinition PG06Weapon;
        public PG06AbilityCatalog PG06Abilities;
        public PG06Ability SelectedPG06Ability;
        public PG06Passive SelectedPG06Passive;

        [Header("PG07")]
        public WeaponDefinition PG07Weapon;
        public PG07AbilityCatalog PG07Abilities;
        public PG07Ability SelectedPG07Ability;
        public PG07Passive SelectedPG07Passive;

        [Header("PG08")]
        public WeaponDefinition PG08Weapon;
        public PG08AbilityCatalog PG08Abilities;
        public PG08Ability SelectedPG08Ability;
        public PG08Passive SelectedPG08Passive;

        [Header("AREA e MOB")]
        public BonusCatalog BonusCatalog;
        public TestAreaSettings GeometryTemplate;
        public MobDefinition ZOMB01;
        [Tooltip("GDD section 14: C1 area totals and MOB distributions.")]
        public int[] AreaTotals = { 130, 160, 200 };
        [Tooltip("Numeri delle AREE BOSS nella sequenza globale della RUN (prima AREA = 1). Escluse dalla crescita delle STATS MOB.")]
        public int[] BossAreaNumbers = new int[0];
        public int OrdinaryAreasBefore(int areaIndex)
        {
            int count = 0;
            for (int index = 0; index < areaIndex; index++)
                if (BossAreaNumbers == null || System.Array.IndexOf(BossAreaNumbers, index + 1) < 0) count++;
            return count;
        }
        public AreaMobDistribution[] AreaMobDistributions = {
            new AreaMobDistribution { Percentages = new float[] { 75, 20, 5, 0, 0 } },
            new AreaMobDistribution { Percentages = new float[] { 65, 25, 10, 0, 0 } },
            new AreaMobDistribution { Percentages = new float[] { 55, 30, 15, 0, 0 } }
        };
        public MobDefinition MobAt(int index) => index == 0 ? ZOMB01 :
            GeometryTemplate != null && GeometryTemplate.Mobs != null && index < GeometryTemplate.Mobs.Length ? GeometryTemplate.Mobs[index] : null;
        [Tooltip("Technical level placement; no final level design is implied.")]
        public Vector2 ExitPosition;
        public AreaLayoutOverride[] AreaLayouts;
        private AreaLayoutOverride Layout(int index)
        {
            if (AreaLayouts != null) foreach (var layout in AreaLayouts)
                if (layout != null && layout.AreaNumber == index + 1) return layout;
            return null;
        }
        public TestAreaSettings GeometryForArea(int index) => Layout(index)?.Geometry ?? GeometryTemplate;
        public Vector2 ExitForArea(int index) => Layout(index)?.ExitPosition ?? ExitPosition;

        public WeaponDefinition SelectedWeapon => SelectedPlayer == LoopPlayer.PG01 ? PG01Weapon :
            SelectedPlayer == LoopPlayer.PG02 ? PG02Weapon : SelectedPlayer == LoopPlayer.PG03 ? PG03Weapon : SelectedPlayer == LoopPlayer.PG04 ? PG04Weapon : SelectedPlayer == LoopPlayer.PG05 ? PG05Weapon : SelectedPlayer == LoopPlayer.PG06 ? PG06Weapon : SelectedPlayer == LoopPlayer.PG07 ? PG07Weapon : PG08Weapon;

        public string Validate()
        {
            if (EnableCompanion && (CompanionPlayer == SelectedPlayer || !System.Enum.IsDefined(typeof(LoopPlayer), CompanionPlayer) || CompanionAbility < 0 || CompanionAbility > 1 || CompanionPassive < 0 || CompanionPassive > 1)) return "Selezionare un PG IA distinto, con una ABILITA e una PASSIVA.";
            var selected = new System.Collections.Generic.HashSet<LoopPlayer> { SelectedPlayer };
            for (int slot = 1; slot <= 3; slot++)
            {
                var member = CompanionAt(slot);
                if (member == null || !member.Enabled) continue;
                if (!System.Enum.IsDefined(typeof(LoopPlayer), member.Player) || !selected.Add(member.Player) || member.Ability < 0 || member.Ability > 1 || member.Passive < 0 || member.Passive > 1)
                    return "Selezionare PG distinti, ciascuno con una ABILITA e una PASSIVA.";
            }
            if (!System.Enum.IsDefined(typeof(LoopPlayer), SelectedPlayer)) return "Selezionare PG01, PG02, PG03, PG04, PG05, PG06, PG07 o PG08.";
            if (GeometryTemplate == null || SelectedWeapon == null || SelectedWeapon.PG == null || ZOMB01 == null)
                return "Assegnare geometria, arma del PG selezionato e ZOMB01.";
            if ((BonusCatalog == null && GeometryTemplate.BonusCatalog == null) || GeometryTemplate.ExperienceThresholds == null || GeometryTemplate.ExperienceThresholds.Length == 0)
                return "Assegnare il catalogo ABILITA BONUS e la tabella EXP.";
            if (SelectedWeapon.PG.PlayerId != SelectedPlayer.ToString() || ZOMB01.Kind != MobKind.ZOMB01)
                return "Arma non corrispondente al PG selezionato o MOB diverso da ZOMB01.";
            if (SelectedPlayer == LoopPlayer.PG01 && (PG01Abilities == null || !PG01Abilities.IsValid || !System.Enum.IsDefined(typeof(PG01Ability), SelectedAbility)))
                return "Assegnare i dati ABILITA PG01 e selezionare PESTONE o BARRIERA.";
            if (SelectedPlayer == LoopPlayer.PG01 && !System.Enum.IsDefined(typeof(PG01Passive), SelectedPassive))
                return "Selezionare esattamente una PASSIVA PG01: PASSIVA 1 o PASSIVA 2.";
            if (SelectedPlayer == LoopPlayer.PG02 && (PG02Abilities == null || !PG02Abilities.IsValid ||
                !System.Enum.IsDefined(typeof(PG02Ability), SelectedPG02Ability) || !System.Enum.IsDefined(typeof(PG02Passive), SelectedPG02Passive)))
                return "Assegnare dati, una ABILITA e una PASSIVA valide per PG02.";
            if (SelectedPlayer == LoopPlayer.PG03 && (PG03Abilities == null || !PG03Abilities.IsValid ||
                !System.Enum.IsDefined(typeof(PG03Ability), SelectedPG03Ability) || !System.Enum.IsDefined(typeof(PG03Passive), SelectedPG03Passive)))
                return "Assegnare dati, una ABILITA e una PASSIVA valide per PG03.";
            if (SelectedPlayer == LoopPlayer.PG04 && (PG04Abilities == null || !PG04Abilities.IsValid || PG04TestItemSlots < 1 || PG04TestItemSlots > 4 ||
                !System.Enum.IsDefined(typeof(PG04Ability), SelectedPG04Ability) || !System.Enum.IsDefined(typeof(PG04Passive), SelectedPG04Passive)))
                return "Assegnare dati, una ABILITA, una PASSIVA e da 1 a 4 SLOT test per PG04.";
            if (SelectedPlayer == LoopPlayer.PG05 && (PG05Abilities == null || !PG05Abilities.IsValid ||
                !System.Enum.IsDefined(typeof(PG05Ability), SelectedPG05Ability) || !System.Enum.IsDefined(typeof(PG05Passive), SelectedPG05Passive)))
                return "Assegnare dati, ABILITA e PASSIVA validi per PG05.";
            if (SelectedPlayer == LoopPlayer.PG06 && (PG06Abilities == null || !PG06Abilities.IsValid ||
                !System.Enum.IsDefined(typeof(PG06Ability), SelectedPG06Ability) || !System.Enum.IsDefined(typeof(PG06Passive), SelectedPG06Passive)))
                return "Assegnare dati, ABILITA e PASSIVA validi per PG06.";
            if (SelectedPlayer == LoopPlayer.PG07 && (PG07Abilities == null || !PG07Abilities.IsValid ||
                !System.Enum.IsDefined(typeof(PG07Ability), SelectedPG07Ability) || !System.Enum.IsDefined(typeof(PG07Passive), SelectedPG07Passive)))
                return "Assegnare dati, ABILITA e PASSIVA validi per PG07.";
            if (SelectedPlayer == LoopPlayer.PG08 && (PG08Abilities == null || !PG08Abilities.IsValid ||
                !System.Enum.IsDefined(typeof(PG08Ability), SelectedPG08Ability) || !System.Enum.IsDefined(typeof(PG08Passive), SelectedPG08Passive)))
                return "Assegnare dati, ABILITA e PASSIVA validi per PG08.";
            if (AreaTotals == null || AreaTotals.Length == 0) return "Configurare almeno una AREA test.";
            foreach (int total in AreaTotals)
                if (total <= 0 || total % 10 != 0) return "Totali positivi multipli di 10: FIRST SPAWN 30% intero.";
            if (AreaMobDistributions == null || AreaMobDistributions.Length < AreaTotals.Length) return "Configurare la distribuzione MOB per ogni AREA.";
            for (int index = 0; index < AreaTotals.Length; index++)
            {
                var weights = AreaMobDistributions[index]?.Percentages;
                if (weights == null || weights.Length != 5) return "Configurare cinque percentuali ZOMB01-ZOMB05 per AREA.";
                float sum = 0;
                for (int type = 0; type < 5; type++)
                {
                    if (float.IsNaN(weights[type]) || float.IsInfinity(weights[type]) || weights[type] < 0) return "Percentuali MOB non valide.";
                    sum += weights[type];
                    if (weights[type] > 0 && (MobAt(type) == null || (int)MobAt(type).Kind != type)) return "Definizione MOB mancante o non corrispondente al tipo.";
                }
                if (Mathf.Abs(sum - 100) > .001f) return "La distribuzione MOB deve totalizzare 100%.";
                var g = GeometryForArea(index); var exit = ExitForArea(index);
                if (g.ActorRadius <= 0 || g.CameraSize <= 0 || g.AreaSize.x <= 0 || g.AreaSize.y <= 0 || g.SearchAttemptsPerFrame < 1)
                    return "Geometria o budget di ricerca non valido.";
                if (!g.EnableMobSeparation) return "Il test richiede collisione/separazione MOB attiva.";
                if (Mathf.Abs(exit.x) + 4 >= g.AreaSize.x / 2 || Mathf.Abs(exit.y) + 4 >= g.AreaSize.y / 2)
                    return "Il TRIGGER di uscita deve essere interno all'AREA.";
            }
            return null;
        }
    }
}
