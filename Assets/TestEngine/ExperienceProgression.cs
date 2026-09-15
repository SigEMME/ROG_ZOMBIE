using UnityEngine;

namespace RogZombie.TestEngine
{
    public sealed class ExperienceProgression : MonoBehaviour
    {
        public int[] Thresholds;
        [SerializeField] private int level = 1;
        [SerializeField] private int experience;
        [SerializeField] private int pendingChoices;
        private BonusInventory inventory;
        private static ExperienceProgression choiceOwner;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetChoices() => choiceOwner = null;
        private float priorTimeScale = 1f;
        public BonusChoice[] Choices { get; private set; }
        public int Level => level;
        public int Experience => experience;
        public int PendingChoices => pendingChoices;
        public int NextThreshold
        {
            get
            {
                int required = Thresholds[Mathf.Min(level, Thresholds.Length) - 1];
                for (int i = Thresholds.Length; i < level; i++) required = (int)System.Math.Floor(required * 1.15 / 10 + .5) * 10;
                return required;
            }
        }
        public int DropPercent = 100;

        private void Awake() => inventory = GetComponent<BonusInventory>();

        public void Award(int amount)
        {
            if (GetComponent<Combatant>().State == LifeState.Dead) return;
            experience += Mathf.CeilToInt(amount * DropPercent / 100f);
            while (experience >= NextThreshold)
            {
                experience -= NextThreshold;
                level++;
                pendingChoices++;
            }

        }

        private void LateUpdate()
        {
            // Finish the current atomic attack (including all its targets) before pausing gameplay.
            if (pendingChoices <= 0 || Choices != null || !GetComponent<Combatant>().IsActive || choiceOwner != null || Time.timeScale <= 0) return;
            choiceOwner = this;
            priorTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            Choices = inventory.Generate();
        }

        public void Choose(int index)
        {
            if (Choices == null || index < 0 || index >= Choices.Length) return;
            inventory.Apply(Choices[index]);
            pendingChoices--;
            Choices = pendingChoices > 0 ? inventory.Generate() : null;
            if (pendingChoices == 0) { Time.timeScale = priorTimeScale; choiceOwner = null; }
        }

        public void CancelChoices() { Choices = null; pendingChoices = 0; if (choiceOwner == this) choiceOwner = null; }

        private void OnDestroy() { if (choiceOwner == this) { choiceOwner = null; if (Choices != null) Time.timeScale = priorTimeScale; } }
    }
}
