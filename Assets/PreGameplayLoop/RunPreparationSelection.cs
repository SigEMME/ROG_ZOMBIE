using UnityEngine;
using RogZombie.TestEngine;
namespace RogZombie.PreGameplayLoop
{
    public enum PreparationPage { Hub, Preparation, Selection, Run }
    public sealed class RunPreparationSelection
    {
        public sealed class Member
        {
            public int Player = -1, Ability = -1, Passive = -1, DescriptionIndex = -1;
            public bool Confirmed;
            public bool Complete => Player >= 0 && Player < 8 && Ability >= 0 && Passive >= 0;
        }
        private readonly Member[] members = { new Member(), new Member(), new Member(), new Member() };
        public Member GetMember(int index) => members[index];
        public int EditingSlot { get; private set; }
        private readonly bool[] enabled = { true, false, false, false };
        public bool CompanionEnabled => enabled[1];
        public bool IsEnabled(int slot) => slot >= 0 && slot < 4 && enabled[slot];
        private Member Current => members[EditingSlot];
        public int Player { get => Current.Player; private set => Current.Player = value; }
        public int Ability { get => Current.Ability; private set => Current.Ability = value; }
        public int Passive { get => Current.Passive; private set => Current.Passive = value; }
        public int DescriptionIndex { get => Current.DescriptionIndex; private set => Current.DescriptionIndex = value; }
        public bool Confirmed { get => Current.Confirmed; private set => Current.Confirmed = value; }
        public PreparationPage Page { get; private set; } = PreparationPage.Hub;
        public bool Complete => Current.Complete;
        public bool CanStart
        {
            get
            {
                if (Page != PreparationPage.Preparation) return false;
                for (int i = 0; i < 4; i++)
                {
                    if (!enabled[i]) continue;
                    if (!members[i].Complete || !members[i].Confirmed) return false;
                    for (int j = 0; j < i; j++) if (enabled[j] && members[i].Player == members[j].Player) return false;
                }
                return true;
            }
        }
        public bool IsAvailable(int player)
        {
            for (int i = 0; i < 4; i++) if (i != EditingSlot && enabled[i] && members[i].Player == player) return false;
            return true;
        }
        public void RemoveCompanion(int slot = 1)
        {
            if (Page != PreparationPage.Preparation || slot < 1 || slot > 3) return;
            enabled[slot] = false; members[slot] = new Member(); EditingSlot = 0;
        }
        public void OpenPreparation() { if (Page == PreparationPage.Hub) Page = PreparationPage.Preparation; }
        public bool OpenBanner(int index)
        {
            if (Page != PreparationPage.Preparation || index < 0 || index > 3) return false;
            EditingSlot = index; enabled[index] = true;
            Confirmed = false; Page = PreparationPage.Selection; return true;
        }
        public void SelectPlayer(int index)
        {
            if (Page != PreparationPage.Selection || index < 0 || index >= 8 || Player == index || !IsAvailable(index)) return;
            Player = index; Ability = Passive = DescriptionIndex = -1; Confirmed = false;
        }
        public void SelectOption(bool passive, int index)
        {
            if (Page != PreparationPage.Selection || Player < 0 || index < 0 || index > 1) return;
            if (passive) Passive = index; else Ability = index;
            DescriptionIndex = (passive ? 2 : 0) + index; Confirmed = false;
        }
        public bool ConfirmSelection()
        {
            if (Page != PreparationPage.Selection || !Complete) return false;
            Confirmed = true; Page = PreparationPage.Preparation; return true;
        }
        public void Back()
        {
            if (Page == PreparationPage.Selection) { Confirmed = false; Page = PreparationPage.Preparation; }
            else if (Page == PreparationPage.Preparation) Page = PreparationPage.Hub;
        }
        public void ReturnToHub() { Page = PreparationPage.Hub; }
        public bool StartRun() { if (!CanStart) return false; Page = PreparationPage.Run; return true; }
        public static WeaponDefinition Weapon(LoopDefinition d, int player)
        {
            switch (player)
            {
                case 0: return d.PG01Weapon; case 1: return d.PG02Weapon; case 2: return d.PG03Weapon; case 3: return d.PG04Weapon;
                case 4: return d.PG05Weapon; case 5: return d.PG06Weapon; case 6: return d.PG07Weapon; case 7: return d.PG08Weapon; default: return null;
            }
        }
        public LoopDefinition CreateRunDefinition(LoopDefinition source)
        {
            if (!CanStart) return null;
            var d = Object.Instantiate(source);
            ApplyMember(d, members[0].Player, members[0].Ability, members[0].Passive);
            d.EnableCompanion = CompanionEnabled;
            d.CompanionPlayer = (LoopPlayer)members[1].Player; d.CompanionAbility = members[1].Ability; d.CompanionPassive = members[1].Passive;
            d.AdditionalCompanions = new CompanionSelection[2];
            for (int i = 2; i < 4; i++) d.AdditionalCompanions[i - 2] = new CompanionSelection
            { Enabled = enabled[i], Player = (LoopPlayer)members[i].Player, Ability = members[i].Ability, Passive = members[i].Passive };
            return d;
        }
        public static void ApplyMember(LoopDefinition d, int player, int ability, int passive)
        {
            d.SelectedPlayer = (LoopPlayer)player;
            switch (player)
            {
                case 0: d.SelectedAbility = (PG01Ability)ability; d.SelectedPassive = (PG01Passive)passive; break;
                case 1: d.SelectedPG02Ability = (PG02Ability)ability; d.SelectedPG02Passive = (PG02Passive)passive; break;
                case 2: d.SelectedPG03Ability = (PG03Ability)ability; d.SelectedPG03Passive = (PG03Passive)passive; break;
                case 3: d.SelectedPG04Ability = (PG04Ability)ability; d.SelectedPG04Passive = (PG04Passive)passive; break;
                case 4: d.SelectedPG05Ability = (PG05Ability)ability; d.SelectedPG05Passive = (PG05Passive)passive; break;
                case 5: d.SelectedPG06Ability = (PG06Ability)ability; d.SelectedPG06Passive = (PG06Passive)passive; break;
                case 6: d.SelectedPG07Ability = (PG07Ability)ability; d.SelectedPG07Passive = (PG07Passive)passive; break;
                case 7: d.SelectedPG08Ability = (PG08Ability)ability; d.SelectedPG08Passive = (PG08Passive)passive; break;
            }
        }
    }
    [System.Serializable] public sealed class PreparationContent { public PreparationCharacter[] Characters; }
    [System.Serializable] public sealed class PreparationCharacter
    {
        public string Name, Description;
        public PreparationOption[] Options;
    }
    [System.Serializable] public sealed class PreparationOption { public string Name, Description; }
}
