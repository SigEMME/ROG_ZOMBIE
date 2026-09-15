using UnityEngine;
using RogZombie.TestEngine;
namespace RogZombie.PreGameplayLoop
{
    public sealed partial class LoopSession
    {
        // Each PG owns its character context; only the root session builds the world.
        public LoopSession ParentSession { get; private set; }
        public LoopSession Companion { get; private set; }
        public LoopSession Controlled { get; private set; }
        public LoopSession World => ParentSession != null ? ParentSession : this;
        private LoopSession rewardContext;
        public LoopSession RewardContext => rewardContext != null ? rewardContext : this;
        public bool DirectlyControlled => World.Controlled == this;
        public LoopSession ChoiceContext => Experience != null && Experience.Choices != null ? this : Companion != null && Companion.Experience.Choices != null ? Companion : this;
        private void PrepareCompanion()
        {
            if (!Definition.EnableCompanion) return;
            if (Companion == null)
            {
                var go = new GameObject("PG IA 1 context"); go.transform.SetParent(transform);
                Companion = go.AddComponent<LoopSession>(); Companion.ParentSession = this;
                Companion.Definition = Instantiate(Definition); Companion.Definition.EnableCompanion = false;
                RunPreparationSelection.ApplyMember(Companion.Definition, (int)Definition.CompanionPlayer, Definition.CompanionAbility, Definition.CompanionPassive);
                Companion.Settings = Settings; Companion.Navigation = Navigation; Companion.GameCamera = GameCamera;
                Companion.CreatePlayer();
                Companion.Player.gameObject.AddComponent<CompanionFormation>().Context = Companion;
            }
            Companion.Settings = Settings; Companion.Navigation = Navigation; Companion.GameCamera = GameCamera;
            Companion.Player.GetComponent<PlayerMovement>().TestSettings = Settings;
        }
        private void PlaceCompanion()
        {
            if (Companion == null) return;
            Vector2 desired = Settings.StartPosition - Vector2.right * CompanionFormation.FormationDistance;
            if (!Companion.Player.GetComponent<CompanionFormation>().TryResolvePosition(Settings.StartPosition, desired, out var point)) return;
            Companion.Player.transform.SetPositionAndRotation(point, Quaternion.identity);
        }
        private void UpdatePartyControl()
        {
            var desired = Player.Actor.IsActive ? this : Companion != null && Companion.Player.Actor.IsActive ? Companion : this;
            if (Controlled != desired) Controlled = desired;
            Player.GetComponent<PlayerMovement>().enabled = Controlled == this;
            if (Companion != null) Companion.Player.GetComponent<PlayerMovement>().enabled = Controlled == Companion;
            if (Exit != null) Exit.SetPlayer(Controlled.Player.Actor);
        }
        public bool PartyReadyForExit()
        {
            return MemberReadyForExit(this) && (Companion == null || MemberReadyForExit(Companion));
        }
        private bool MemberReadyForExit(LoopSession member)
        {
            var actor = member.Player.Actor;
            if (actor.State == LifeState.Dead) return true;
            return actor.IsActive && member.Experience.PendingChoices == 0 && Exit != null &&
                Vector2.Distance(actor.transform.position, Exit.transform.position) <= AreaExitTrigger.Radius;
        }
        public void TickResurrection(float seconds, bool held)
        {
            if (ParentSession != null || !GameplayRunning || seconds <= 0) return;
            UpdatePartyControl();
            var rescuer = Controlled.Player.Actor;
            TickMemberResurrection(this, rescuer, seconds, held);
            if (Companion != null) TickMemberResurrection(Companion, rescuer, seconds, held);
            UpdatePartyControl();
        }
        private static void TickMemberResurrection(LoopSession member, Combatant rescuer, float seconds, bool held)
        {
            var target = member.Player.Actor;
            bool valid = held && rescuer != target && rescuer.IsActive &&
                Vector2.Distance(rescuer.transform.position, target.transform.position) <= ResurrectionRuntime.Radius;
            member.Resurrection.Tick(seconds, valid);
        }
        private static void RestoreMemberForArea(LoopSession member)
        {
            if (member.Player.Actor.State == LifeState.Dead) member.Player.Actor.Resurrect();
            else member.Player.Actor.Heal(.15f);
        }
        private void DrawAreaChoices()
        {
            Choices = AreaStatBonus.Draw();
            AbilityChoices = RewardContext.Bonuses.Generate(2, System.Array.ConvertAll(Choices, stat => (int)stat));
            Selected = -1;
        }
    }
}
