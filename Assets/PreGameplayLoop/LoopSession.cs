using System.Collections;
using UnityEngine;
using RogZombie.TestEngine;

namespace RogZombie.PreGameplayLoop
{
    public enum LoopState { Loading, Combat, AreaComplete, Bonus, Transition, Finished, Defeat, Error }

    [DefaultExecutionOrder(-100)]
    public sealed class LoopSession : MonoBehaviour, ISpawnWorld
    {
        public LoopDefinition Definition;
        public LoopState State { get; private set; } = LoopState.Loading;
        public Camera GameCamera { get; private set; }
        public PlayerRuntime Player { get; private set; }
        public SpawnManager Spawns { get; private set; }
        public TestNavigation Navigation { get; private set; }
        public AreaExitTrigger Exit { get; private set; }
        public TestAreaSettings Settings { get; private set; }
        public bool Loading => State == LoopState.Loading || State == LoopState.Transition;
        public int AreaIndex { get; private set; }
        public int Gold { get; private set; }
        public float CdReduction => cdReduction;
        public PG01AbilityRuntime Ability { get; private set; }
        public bool GameplayRunning => Time.timeScale > 0 && (State == LoopState.Combat || State == LoopState.AreaComplete);
        public string Failure { get; private set; }
        public AreaStat[] Choices { get; private set; }
        public int Selected { get; private set; } = -1;
        private float cdReduction;
        private Transform areaRoot;
        private float previousTimeScale;
        private WeaponDefinition runtimeWeapon;

        private void Start()
        {
            previousTimeScale = Time.timeScale;
            GameCamera = Camera.main;
            gameObject.AddComponent<LoopHUD>();
            string issue = Definition == null ? "LoopDefinition mancante." : Definition.Validate();
            foreach (string layer in new[] { "PG", "MOB", "MURO", "OSTACOLO", "TRIGGER_PG" })
                if (LayerMask.NameToLayer(layer) < 0) issue = "Layer mancante: " + layer;
            if (GameCamera == null) issue = "Main Camera mancante.";
            if (issue != null) { Fail(issue); return; }
            StartCoroutine(BuildArea());
        }

        private void Fail(string issue)
        {
            Failure = issue;
            State = LoopState.Error;
            Time.timeScale = 0;
            Debug.LogError("Pre gameplay loop: " + issue, this);
        }

        private IEnumerator BuildArea()
        {
            State = LoopState.Loading;
            Time.timeScale = 0;
            Settings = Instantiate(Definition.GeometryTemplate);
            Settings.TotalMobs = Definition.AreaTotals[AreaIndex];
            Settings.FirstSpawnPercent = 30;
            Settings.MobPercentages = new float[] { 100, 0, 0, 0, 0 };
            Settings.OffscreenExtraRange = 5;
            Settings.PlayerMoveSpeedBase = 2; // GDD unit conversion, supersedes old test tuning only here.
            Settings.Mobs = new[] { Definition.ZOMB01 };
            GameCamera.orthographic = true;
            GameCamera.orthographicSize = Settings.CameraSize;
            areaRoot = new GameObject("AREA test " + (AreaIndex + 1)).transform;
            areaRoot.SetParent(transform);
            TestVisuals.Root = areaRoot;
            BuildGeometry();
            Navigation = areaRoot.gameObject.AddComponent<TestNavigation>();
            Navigation.AgentRadius = Settings.ActorRadius;
            Navigation.Build(Settings.AreaSize);
            if (!Navigation.Ready || !Navigation.Sample(Settings.StartPosition, out var start, .1f) ||
                !Navigation.Sample(Definition.ExitPosition, out var exit, .1f) || !Navigation.Reachable(start, exit))
            { Fail("Inizio/uscita non validi o non collegati sulla NavMesh."); yield break; }
            if (Player == null) CreatePlayer();
            Player.GetComponent<PlayerMovement>().TestSettings = Settings;
            Player.transform.SetPositionAndRotation(Settings.StartPosition, Quaternion.identity);
            FollowCamera();
            var exitObject = TestVisuals.Box("USCITA AREA", Definition.ExitPosition, Vector2.one, Color.gray, 1);
            Exit = exitObject.AddComponent<AreaExitTrigger>();
            Exit.Initialize(Player.Actor);
            Exit.Entered += OpenBonus;
            var debug = areaRoot.gameObject.AddComponent<DamageNumbersDebug>();
            debug.Settings = Settings;
            debug.ViewCamera = GameCamera;
            Spawns = areaRoot.gameObject.AddComponent<SpawnManager>();
            Spawns.Initialize(Settings, Navigation, this);
            // Unscaled diagnostic timeout is technical, not a spawn gameplay rule.
            float started = Time.realtimeSinceStartup;
            while (!Spawns.InitialComplete)
            {
                if (Time.realtimeSinceStartup - started > 60)
                { Fail("FIRST SPAWN bloccato: verificare spazio off-screen, camera e geometria."); yield break; }
                yield return null;
            }
            State = LoopState.Combat;
            Time.timeScale = 1;
        }

        private void BuildGeometry()
        {
            Vector2 size = Settings.AreaSize;
            TestVisuals.Box("Pavimento test", Vector2.zero, size,
                AreaIndex == 0 ? new Color(.09f, .12f, .13f) : new Color(.13f, .10f, .09f), -10);
            MakeObstacle(new Vector2(-size.x / 2, 0), new Vector2(1, size.y), true);
            MakeObstacle(new Vector2(size.x / 2, 0), new Vector2(1, size.y), true);
            MakeObstacle(new Vector2(0, -size.y / 2), new Vector2(size.x, 1), true);
            MakeObstacle(new Vector2(0, size.y / 2), new Vector2(size.x, 1), true);
            if (Settings.Obstacles != null)
                foreach (var obstacle in Settings.Obstacles) MakeObstacle(obstacle.Position, obstacle.Size, obstacle.Wall);
        }

        private void MakeObstacle(Vector2 position, Vector2 size, bool wall)
        {
            string layer = wall ? "MURO" : "OSTACOLO";
            var go = TestVisuals.Box(layer, position, size, new Color(.35f, .4f, .43f), 1);
            go.layer = LayerMask.NameToLayer(layer);
            go.AddComponent<BoxCollider2D>().size = size;
            go.AddComponent<TestObstacle>().IsWall = wall;
        }

        private void CreatePlayer()
        {
            var go = TestVisuals.Box("PG01", Settings.StartPosition, Vector2.one, Color.white, 3);
            go.SetActive(false);
            go.transform.SetParent(transform); // Keep runtime HP/stats across area teardown.
            go.layer = LayerMask.NameToLayer("PG");
            if (Settings.PlayerSprite != null)
            {
                var sprite = go.GetComponent<SpriteRenderer>();
                sprite.drawMode = SpriteDrawMode.Simple;
                sprite.sprite = Settings.PlayerSprite;
            }
            go.AddComponent<CircleCollider2D>().radius = Settings.ActorRadius;
            go.AddComponent<Combatant>();
            go.AddComponent<PlayerMovement>();
            go.AddComponent<PlayerAim>();
            Player = go.AddComponent<PlayerRuntime>();
            Player.Initialize(Definition.PG01Weapon.PG);
            cdReduction = Player.CdReduction;
            var muzzle = new GameObject("Bocca arma").transform;
            muzzle.SetParent(go.transform, false);
            muzzle.localPosition = new Vector3(.5f, 0, 0); // Existing test muzzle placement.
            var weapon = go.AddComponent<PlayerWeapon>();
            if (runtimeWeapon != null) Destroy(runtimeWeapon);
            runtimeWeapon = Instantiate(Definition.PG01Weapon);
            runtimeWeapon.ConeAngle = 75; // GDD PG01: 4m radial RANGE, 75 degrees.
            weapon.Definition = runtimeWeapon;
            weapon.Muzzle = muzzle;
            Ability = go.AddComponent<PG01AbilityRuntime>();
            Ability.Initialize(this, Definition.SelectedAbility, Definition.PG01Abilities);
            go.AddComponent<PG01AbilityInput>();
            go.SetActive(true);
        }

        public Combatant CreateMob(int index, Vector2 position)
        {
            var go = TestVisuals.Box("ZOMB01", position, Vector2.one, new Color(.4f, .7f, .4f), 2);
            go.layer = LayerMask.NameToLayer("MOB");
            go.AddComponent<CircleCollider2D>().radius = Settings.ActorRadius;
            var actor = go.AddComponent<Combatant>();
            go.AddComponent<MobBrain>().Initialize(Definition.ZOMB01, Navigation);
            go.AddComponent<MobSeparation>().Settings = Settings;
            return actor;
        }

        private void Update()
        {
            if (State != LoopState.Combat && State != LoopState.AreaComplete) return;
            if (!Player.Actor.IsActive)
            {
                State = LoopState.Defeat;
                Time.timeScale = 0; // Sole PG DOWN means no active party member remains.
                return;
            }
            if (State == LoopState.Combat && Spawns.Killed == Settings.TotalMobs &&
                Spawns.TotalSpawned == Settings.TotalMobs && Spawns.Alive == 0)
            {
                State = LoopState.AreaComplete;
                Exit.Open();
            }
        }

        private void OpenBonus()
        {
            if (State != LoopState.AreaComplete || !Player.Actor.IsActive) return;
            State = LoopState.Bonus;
            Time.timeScale = 0;
            Choices = AreaStatBonus.Draw();
            Selected = -1;
        }

        public void SelectBonus(int index)
        {
            if (State == LoopState.Bonus && Choices != null && index >= 0 && index < Choices.Length) Selected = index;
        }

        public void ConfirmBonus()
        {
            if (State != LoopState.Bonus || Selected < 0) return;
            AreaStatBonus.Apply(Player.Actor, Choices[Selected], ref cdReduction);
            Choices = null;
            Selected = -1;
            if (AreaIndex == Definition.AreaTotals.Length - 1)
            { State = LoopState.Finished; return; } // Test boundary; not a city/run victory.
            State = LoopState.Transition; // Guard double confirmation before yielding.
            StartCoroutine(NextArea());
        }

        private IEnumerator NextArea()
        {
            Ability.ChangeArea();
            Exit.Entered -= OpenBonus;
            areaRoot.gameObject.SetActive(false);
            Destroy(areaRoot.gameObject);
            Destroy(Settings);
            yield return null; // Dispose corpse timers, effects, NavMesh and spawn subscriptions.
            Player.Actor.Heal(.15f);
            AreaIndex++;
            yield return BuildArea();
        }

        public void AddGold(int value) => Gold += value;
        public void RefreshNavigation()
        {
            if (Navigation != null && Settings != null) Navigation.Build(Settings.AreaSize);
        }
        public void RestartTest()
        {
            if (Loading) return;
            StopAllCoroutines();
            State = LoopState.Transition;
            Time.timeScale = 0;
            StartCoroutine(RestartRoutine());
        }

        private IEnumerator RestartRoutine()
        {
            if (areaRoot != null) { areaRoot.gameObject.SetActive(false); Destroy(areaRoot.gameObject); }
            if (Player != null) { Player.gameObject.SetActive(false); Destroy(Player.gameObject); }
            if (Settings != null) Destroy(Settings);
            yield return null;
            Player = null;
            AreaIndex = 0;
            Gold = 0;
            Choices = null;
            Selected = -1;
            Failure = null;
            yield return BuildArea();
        }
        private void LateUpdate() => FollowCamera();
        private void FollowCamera()
        {
            if (Player != null && GameCamera != null)
                GameCamera.transform.position = new Vector3(Player.transform.position.x, Player.transform.position.y, -10);
        }

        private void OnDestroy()
        {
            if (Exit != null) Exit.Entered -= OpenBonus;
            if (Settings != null) Destroy(Settings);
            if (TestVisuals.Root == areaRoot) TestVisuals.Root = null;
            if (runtimeWeapon != null) Destroy(runtimeWeapon);
            Time.timeScale = previousTimeScale;
        }
    }
}
