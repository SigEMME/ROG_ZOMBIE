using UnityEngine;
using RogZombie.TestEngine;
namespace RogZombie.PreGameplayLoop
{
    public enum PreparationPage { Hub, Preparation, Selection, Run }
    public sealed class RunPreparationSelection
    {
        public int Player { get; private set; } = -1;
        public int Ability { get; private set; } = -1;
        public int Passive { get; private set; } = -1;
        public int DescriptionIndex { get; private set; } = -1;
        public bool Confirmed { get; private set; }
        public PreparationPage Page { get; private set; } = PreparationPage.Hub;
        public bool Complete => Player >= 0 && Player < 8 && Ability >= 0 && Passive >= 0;
        public bool CanStart => Page == PreparationPage.Preparation && Complete && Confirmed;
        public void OpenPreparation() { if (Page == PreparationPage.Hub) Page = PreparationPage.Preparation; }
        public bool OpenBanner(int index)
        {
            if (Page != PreparationPage.Preparation || index != 0) return false;
            Confirmed = false; Page = PreparationPage.Selection; return true;
        }
        public void SelectPlayer(int index)
        {
            if (Page != PreparationPage.Selection || index < 0 || index >= 8 || Player == index) return;
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
            var d = Object.Instantiate(source); d.SelectedPlayer = (LoopPlayer)Player;
            switch (Player)
            {
                case 0: d.SelectedAbility = (PG01Ability)Ability; d.SelectedPassive = (PG01Passive)Passive; break;
                case 1: d.SelectedPG02Ability = (PG02Ability)Ability; d.SelectedPG02Passive = (PG02Passive)Passive; break;
                case 2: d.SelectedPG03Ability = (PG03Ability)Ability; d.SelectedPG03Passive = (PG03Passive)Passive; break;
                case 3: d.SelectedPG04Ability = (PG04Ability)Ability; d.SelectedPG04Passive = (PG04Passive)Passive; break;
                case 4: d.SelectedPG05Ability = (PG05Ability)Ability; d.SelectedPG05Passive = (PG05Passive)Passive; break;
                case 5: d.SelectedPG06Ability = (PG06Ability)Ability; d.SelectedPG06Passive = (PG06Passive)Passive; break;
                case 6: d.SelectedPG07Ability = (PG07Ability)Ability; d.SelectedPG07Passive = (PG07Passive)Passive; break;
                case 7: d.SelectedPG08Ability = (PG08Ability)Ability; d.SelectedPG08Passive = (PG08Passive)Passive; break;
            }
            return d;
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
