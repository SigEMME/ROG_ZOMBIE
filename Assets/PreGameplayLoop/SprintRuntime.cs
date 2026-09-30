using UnityEngine;
using UnityEngine.InputSystem;
using RogZombie.TestEngine;

namespace RogZombie.PreGameplayLoop
{
    [DefaultExecutionOrder(-50)]
    public sealed class SprintRuntime : MonoBehaviour
    {
        public const float Duration = 2f;
        public const float Recovery = 15f;
        private LoopSession session;
        private Combatant actor;
        public float ActiveRemaining { get; private set; }
        public float CooldownRemaining { get; private set; }
        public bool Active => ActiveRemaining > 0;
        public float SpeedMultiplier => Active && actor != null && actor.IsActive ? 2f : 1f;
        public void Initialize(LoopSession owner)
        {
            session = owner;
            actor = GetComponent<Combatant>();
            actor.StateChanged += OnState;
        }
        public bool TryActivate()
        {
            if (session == null || !session.DirectlyControlled || !session.GameplayRunning || session.PauseMenuOpen ||
                actor == null || !actor.IsActive || RogZombie.BossTest.BossPlayerStatus.Blocks(this) || Active || CooldownRemaining > 0)
                return false;
            actor.NotifyAction();
            ActiveRemaining = Duration;
            return true;
        }
        private void Update()
        {
            if (session == null || !session.GameplayRunning) return;
            Advance(Time.deltaTime);
            var keyboard = Keyboard.current;
            if (Application.isFocused && keyboard != null &&
                (keyboard.leftShiftKey.wasPressedThisFrame || keyboard.rightShiftKey.wasPressedThisFrame)) TryActivate();
        }
        private void Advance(float seconds)
        {
            if (Active)
            {
                float consumed = Mathf.Min(seconds, ActiveRemaining);
                ActiveRemaining = Mathf.Max(0, ActiveRemaining - consumed);
                seconds -= consumed;
                if (!Active) CooldownRemaining = Recovery;
            }
            CooldownRemaining = Mathf.Max(0, CooldownRemaining - seconds);
        }
        public void ChangeArea()
        {
            if (!Active) return;
            ActiveRemaining = 0;
            CooldownRemaining = Recovery;
        }
        private void OnState(Combatant changed) { if (!changed.IsActive) ChangeArea(); }
        private void OnDestroy() { if (actor != null) actor.StateChanged -= OnState; }
    }
}
