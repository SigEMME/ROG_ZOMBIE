using UnityEngine;
using RogZombie.TestEngine;
namespace RogZombie.PreGameplayLoop
{
    public sealed partial class LoopSession
    {
        // Each PG owns its character context; only the root session builds the world.
        public LoopSession ParentSession { get; private set; }
        private readonly System.Collections.Generic.List<LoopSession> companions = new System.Collections.Generic.List<LoopSession>(3);
        public System.Collections.Generic.IReadOnlyList<LoopSession> Companions => companions;
        // Compatibility with existing single-companion callers and regression fixtures.
        public LoopSession Companion => companions.Count > 0 ? companions[0] : null;
        public int PartySlot { get; private set; }
        public LoopSession Controlled { get; private set; }
        public LoopSession World => ParentSession != null ? ParentSession : this;
        private LoopSession rewardContext;
        public LoopSession RewardContext => rewardContext != null ? rewardContext : this;
        public bool DirectlyControlled => World.Controlled == this;
        public System.Collections.Generic.IEnumerable<LoopSession> Members
        {
            get { yield return this; foreach (var member in companions) yield return member; }
        }
        public LoopSession ChoiceContext
        {
            get { foreach (var member in Members) if (member.Experience != null && member.Experience.Choices != null) return member; return this; }
        }
        private void PrepareCompanion()
        {
            if (companions.Count == 0)
            {
                for (int slot = 1; slot <= 3; slot++)
                {
                    var selection = Definition.CompanionAt(slot);
                    if (selection == null || !selection.Enabled) continue;
                    var go = new GameObject("PG IA " + slot + " context"); go.transform.SetParent(transform);
                    var member = go.AddComponent<LoopSession>(); member.ParentSession = this; member.PartySlot = slot;
                    member.Definition = Instantiate(Definition); member.Definition.EnableCompanion = false;
                    member.Definition.AdditionalCompanions = new CompanionSelection[0];
                    RunPreparationSelection.ApplyMember(member.Definition, (int)selection.Player, selection.Ability, selection.Passive);
                    string issue = member.Definition.Validate();
                    if (issue != null) { Destroy(go); Fail("PG IA " + slot + ": " + issue); return; }
                    member.Settings = Settings; member.Navigation = Navigation; member.GameCamera = GameCamera;
                    member.CreatePlayer();
                    member.Player.gameObject.AddComponent<CompanionFormation>().Context = member;
                    companions.Add(member);
                }
            }
            foreach (var member in companions)
            {
                member.Settings = Settings; member.Navigation = Navigation; member.GameCamera = GameCamera;
                member.Player.GetComponent<PlayerMovement>().TestSettings = Settings;
            }
        }
        public Vector2 FormationOffset(LoopSession follower)
        {
            int count = 0, index = -1;
            foreach (var member in Members)
            {
                if (member == Controlled || !member.Player.Actor.IsActive) continue;
                if (member == follower) index = count;
                count++;
            }
            return CompanionFormation.Offset(count, Mathf.Max(0, index));
        }
        private void PlaceCompanion()
        {
            UpdatePartyControl();
            // All surviving actors start on valid navigation before resolving their distinct slots.
            foreach (var member in companions) member.Player.transform.position = Settings.StartPosition;
            foreach (var member in companions)
            {
                Vector2 desired = Settings.StartPosition + FormationOffset(member);
                var formation = member.Player.GetComponent<CompanionFormation>();
                if (!formation.TryResolvePosition(Settings.StartPosition, desired, out var point)) continue;
                member.Player.transform.SetPositionAndRotation(point, Quaternion.identity);
                Physics2D.SyncTransforms();
            }
        }
        private void UpdatePartyControl()
        {
            var desired = this;
            foreach (var member in Members) if (member.Player != null && member.Player.Actor.IsActive) { desired = member; break; }
            Controlled = desired;
            foreach (var member in Members) if (member.Player != null) member.Player.GetComponent<PlayerMovement>().enabled = Controlled == member;
            if (Exit != null) Exit.SetPlayer(Controlled.Player.Actor);
        }
        public bool PartyReadyForExit()
        {
            foreach (var member in Members) if (!MemberReadyForExit(member)) return false;
            return true;
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
            LoopSession nearest = null;
            float best = ResurrectionRuntime.Radius * ResurrectionRuntime.Radius;
            if (held && rescuer.IsActive)
                foreach (var member in Members)
                {
                    if (member.Player.Actor.State != LifeState.Down) continue;
                    float distance = ((Vector2)(rescuer.transform.position - member.Player.transform.position)).sqrMagnitude;
                    if (distance > best || nearest != null && distance == best) continue;
                    best = distance; nearest = member;
                }
            foreach (var member in Members) member.Resurrection.Tick(seconds, member == nearest);
            UpdatePartyControl();
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
