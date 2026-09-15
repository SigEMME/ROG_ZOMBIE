using UnityEngine;
using RogZombie.TestEngine;
namespace RogZombie.PreGameplayLoop
{
    public sealed class ResurrectionRuntime : MonoBehaviour
    {
        public const float DownDuration = 20f, ReviveDuration = 5f, Radius = 2f;
        public float DownRemaining { get; private set; }
        public float Progress { get; private set; }
        public bool Interacting { get; private set; }
        private Combatant actor;
        private CircleCollider2D reviveTrigger;
        private void Awake()
        {
            actor = GetComponent<Combatant>();
            var triggerObject = new GameObject("RIANIMAZIONE — 2 m");
            triggerObject.transform.SetParent(transform, false);
            triggerObject.layer = LayerMask.NameToLayer("TRIGGER_PG");
            reviveTrigger = triggerObject.AddComponent<CircleCollider2D>();
            reviveTrigger.radius = Radius; reviveTrigger.isTrigger = true;
            actor.StateChanged += StateChanged;
            StateChanged(actor);
        }
        private void StateChanged(Combatant value)
        {
            DownRemaining = value.State == LifeState.Down ? DownDuration : 0;
            Progress = 0; Interacting = false;
            reviveTrigger.enabled = value.State == LifeState.Down;
        }
        public void Tick(float seconds, bool interacting)
        {
            if (seconds <= 0) return;
            if (actor.State != LifeState.Down) { Interacting = false; return; }
            Interacting = interacting;
            if (interacting)
            {
                Progress = Mathf.Min(ReviveDuration, Progress + seconds);
                if (Progress >= ReviveDuration) actor.Resurrect();
            }
            else
            {
                Progress = Mathf.Max(0, Progress - seconds);
                DownRemaining = Mathf.Max(0, DownRemaining - seconds);
                if (DownRemaining <= 0) actor.Die();
            }
        }
        private void OnDestroy() { if (actor != null) actor.StateChanged -= StateChanged; }
    }
}
