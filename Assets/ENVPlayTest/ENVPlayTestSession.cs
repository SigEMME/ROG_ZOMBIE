using UnityEngine;
using RogZombie.TestEngine;

namespace RogZombie.PlayTests
{
    // Isolated test wiring; AI, movement, aiming and combat come from the existing systems.
    [DefaultExecutionOrder(-200)]
    public sealed class ENVPlayTestSession : MonoBehaviour
    {
        public PlayerRuntime Player;
        public Combatant Zombie;
        public WeaponDefinition Weapon;
        public MobDefinition ZombieDefinition;
        public Transform EffectsRoot;
        public bool RunZombieAI = true;
        private Vector3 playerStart, zombieStart;
        private GameObject template;
        private TestNavigation navigation;
        public MobBrain Brain => Zombie != null ? Zombie.GetComponent<MobBrain>() : null;

        private void Start()
        {
            if (Player == null || Zombie == null || Weapon == null || Weapon.PG == null || ZombieDefinition == null || EffectsRoot == null)
            {
                Debug.LogError("ENV PlayTest: missing scene references.", this);
                enabled = false;
                return;
            }
            playerStart = Player.transform.position;
            zombieStart = Zombie.transform.position;
            TestVisuals.Root = EffectsRoot;
            navigation = gameObject.AddComponent<TestNavigation>();
            navigation.AgentRadius = Zombie.GetComponent<CircleCollider2D>().radius;
            navigation.Build(new Vector2(16f, 10f));
            template = Instantiate(Zombie.gameObject);
            template.name = "ZOMB01 reset template (inactive)";
            template.SetActive(false);
            template.hideFlags = HideFlags.HideInHierarchy;
            ResetActors();
        }

        public void SetZombieAI(bool value)
        {
            RunZombieAI = value;
            if (Brain != null) Brain.enabled = value;
        }

        public void ResetActors()
        {
            if (template == null) return;
            Player.transform.SetPositionAndRotation(playerStart, Quaternion.identity);
            Player.Initialize(Weapon.PG);
            // A new instance avoids reviving an actor already scheduled for corpse removal.
            if (Zombie != null) { Zombie.gameObject.SetActive(false); Destroy(Zombie.gameObject); }
            var fresh = Instantiate(template, zombieStart, Quaternion.identity);
            fresh.hideFlags = HideFlags.None;
            fresh.name = "ZOMB01 - existing AI and animated presentation";
            Zombie = fresh.GetComponent<Combatant>();
            fresh.SetActive(true);
            Brain.Initialize(ZombieDefinition, navigation);
            SetZombieAI(RunZombieAI);
            Physics2D.SyncTransforms();
        }

        private void OnGUI()
        {
            if (Player == null || Player.Actor == null) return;
            GUI.Box(new Rect(8, 8, 690, 94), "ENV / ZOMB01 / PG01 - PLAY TEST");
            GUI.Label(new Rect(20, 30, 665, 22), "WASD | Mouse + sinistro: attacco | PG01 HP: " + Player.Actor.CurrentHP);
            string status = Zombie != null ? Zombie.CurrentHP + " HP / " + Zombie.State : "MORTE: rimosso dopo 3 s";
            GUI.Label(new Rect(20, 49, 665, 22), "ZOMB01: " + status + " | " + (RunZombieAI ? "AI attiva" : "Fermo per IDLE / prova HIT"));
            if (GUI.Button(new Rect(20, 73, 150, 22), "Ripristina attori")) ResetActors();
            bool active = GUI.Toggle(new Rect(190, 73, 220, 22), RunZombieAI, "AI ZOMB01 attiva");
            if (active != RunZombieAI) SetZombieAI(active);
            GUI.Label(new Rect(12, Screen.height - 32, 850, 25), "Area urbana: props solidi, passaggi e profondita. PG01 usa il placeholder esistente.");
        }

        private void OnDestroy()
        {
            if (template != null) Destroy(template);
            if (TestVisuals.Root == EffectsRoot) TestVisuals.Root = null;
        }
    }
}
