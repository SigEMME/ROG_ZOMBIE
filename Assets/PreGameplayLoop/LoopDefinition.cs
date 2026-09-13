using UnityEngine;
using RogZombie.TestEngine;

namespace RogZombie.PreGameplayLoop
{
    public enum LoopPlayer { PG01, PG02, PG03, PG04, PG05, PG06, PG07, PG08 }
    [CreateAssetMenu(menuName = "ROG ZOMBIE/Pre gameplay loop")]
    public sealed class LoopDefinition : ScriptableObject
    {
        [Header("RUN")]
        public LoopPlayer SelectedPlayer = LoopPlayer.PG01;

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
        public TestAreaSettings GeometryTemplate;
        public MobDefinition ZOMB01;
        [Tooltip("GDD section 14: C1 A1 and A2 totals. ZOMB01-only test composition.")]
        public int[] AreaTotals = { 100, 120 };
        [Tooltip("Technical level placement; no final level design is implied.")]
        public Vector2 ExitPosition;

        public WeaponDefinition SelectedWeapon => SelectedPlayer == LoopPlayer.PG01 ? PG01Weapon :
            SelectedPlayer == LoopPlayer.PG02 ? PG02Weapon : SelectedPlayer == LoopPlayer.PG03 ? PG03Weapon : SelectedPlayer == LoopPlayer.PG04 ? PG04Weapon : SelectedPlayer == LoopPlayer.PG05 ? PG05Weapon : SelectedPlayer == LoopPlayer.PG06 ? PG06Weapon : SelectedPlayer == LoopPlayer.PG07 ? PG07Weapon : PG08Weapon;

        public string Validate()
        {
            if (!System.Enum.IsDefined(typeof(LoopPlayer), SelectedPlayer)) return "Selezionare PG01, PG02, PG03, PG04, PG05, PG06, PG07 o PG08.";
            if (GeometryTemplate == null || SelectedWeapon == null || SelectedWeapon.PG == null || ZOMB01 == null)
                return "Assegnare geometria, arma del PG selezionato e ZOMB01.";
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
