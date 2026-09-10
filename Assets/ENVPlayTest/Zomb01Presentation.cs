using UnityEngine;
using RogZombie.TestEngine;

namespace RogZombie.PlayTests
{
    // Observes gameplay. Animation never applies damage, delays attacks or stops movement.
    [RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
    [DefaultExecutionOrder(200)]
    public sealed class Zomb01Presentation : MonoBehaviour
    {
        public AnimationClip AttackClip;
        private Combatant actor;
        private MobBrain brain;
        private Animator animator;
        private SpriteRenderer body;
        private Vector3 previousPosition;

        private void Awake()
        {
            actor = GetComponentInParent<Combatant>();
            brain = GetComponentInParent<MobBrain>();
            animator = GetComponent<Animator>();
            body = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            if (actor == null || brain == null || AttackClip == null)
            {
                Debug.LogError("ZOMB01 presentation requires actor, brain and attack clip.", this);
                enabled = false;
                return;
            }
            previousPosition = actor.transform.position;
            animator.speed = 1f; // IDLE alone uses 8/60 in its Animator state.
            brain.MeleeAttackPerformed += OnAttack;
            actor.StateChanged += OnState;
        }

        private void OnDisable()
        {
            if (brain != null) brain.MeleeAttackPerformed -= OnAttack;
            if (actor != null) actor.StateChanged -= OnState;
        }

        private void LateUpdate()
        {
            if (Time.timeScale <= 0f) return;
            Vector3 delta = actor.transform.position - previousPosition;
            previousPosition = actor.transform.position;
            animator.SetBool("Moving", actor.IsActive && delta.sqrMagnitude > 0.0000001f);
            if (actor.IsActive && Mathf.Abs(delta.x) > 0.00001f) body.flipX = delta.x < 0f;
        }

        private void OnAttack(Combatant target)
        {
            if (!actor.IsActive) return;
            float horizontal = target.transform.position.x - actor.transform.position.x;
            if (Mathf.Abs(horizontal) > 0.00001f) body.flipX = horizontal < 0f;
            animator.SetFloat("AttackPlayback", AttackClip.length / actor.Stats.AttackInterval);
            animator.SetTrigger("Attack");
        }

        private void OnState(Combatant changed)
        {
            if (changed.State != LifeState.Dead) return;
            animator.ResetTrigger("Attack");
            animator.SetBool("Dead", true);
        }
    }
}
