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
        private bool warnedTableEnd;
        private float priorTimeScale = 1f;
        public BonusChoice[] Choices { get; private set; }
        public int Level => level;
        public int Experience => experience;
        public int PendingChoices => pendingChoices;
        public int NextThreshold => level <= Thresholds.Length ? Thresholds[level - 1] : 0;

        private void Awake() => inventory = GetComponent<BonusInventory>();

        public void Award(int amount)
        {
            if (GetComponent<Combatant>().State == LifeState.Dead) return;
            experience += amount;
            while (level <= Thresholds.Length && experience >= Thresholds[level - 1])
            {
                experience -= Thresholds[level - 1];
                level++;
                pendingChoices++;
            }
            if (level > Thresholds.Length && !warnedTableEnd)
            {
                warnedTableEnd = true;
                Debug.LogWarning("EXP table ends at LVL 11. Residual EXP is retained; no undefined thresholds are extrapolated.", this);
            }
        }

        private void LateUpdate()
        {
            // Finish the current atomic attack (including all its targets) before pausing gameplay.
            if (pendingChoices <= 0 || Choices != null) return;
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
            if (pendingChoices == 0) Time.timeScale = priorTimeScale;
        }

        private void OnDestroy() { if (Choices != null) Time.timeScale = priorTimeScale; }
    }
}
