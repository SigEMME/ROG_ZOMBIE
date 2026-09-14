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
        public PG01PassiveRuntime Passive { get; private set; }
        public PG02AbilityRuntime PG02Ability { get; private set; }
        public PG02PassiveRuntime PG02Passive { get; private set; }
        public PG03AbilityRuntime PG03Ability { get; private set; }
        public PG03PassiveRuntime PG03Passive { get; private set; }
        public PG04AbilityRuntime PG04Ability { get; private set; }
        public PG08AbilityRuntime PG08Ability { get; private set; }
        public PG08PassiveRuntime PG08Passive { get; private set; }
        public PG07AbilityRuntime PG07Ability { get; private set; }
        public PG06AbilityRuntime PG06Ability { get; private set; }
        public PG05AbilityRuntime PG05Ability { get; private set; }
        public PG04ItemSlots PG04Items { get; private set; }
        public bool GameplayRunning => Time.timeScale > 0 && (State == LoopState.Combat || State == LoopState.AreaComplete);
        public string Failure { get; private set; }
        public BonusInventory Bonuses { get; private set; }
        public BonusAbilityRuntime BonusAbilities { get; private set; }
        public ExperienceProgression Experience { get; private set; }
        public BonusChoice[] AbilityChoices { get; private set; }
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
            foreach (string layer in new[] { "PG", "MOB", "MURO", "OSTACOLO", "TRIGGER_PG", "TRIGGER_MOB", "PET" })
                if (LayerMask.NameToLayer(layer) < 0) issue = "Layer mancante: " + layer;
            if (GameCamera == null) issue = "Main Camera mancante.";
            if (issue != null) { Fail(issue); return; }
            gameObject.AddComponent<HealingNumbers>().ViewCamera = GameCamera;
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
            TestVisuals.FloorGrid(size);
            TestVisuals.Box("SPAWN PG", Settings.StartPosition, Vector2.one, Color.green, -8);
            TestVisuals.Box("Riferimento USCITA", Definition.ExitPosition, Vector2.one * 1.5f, Color.yellow, -8);
            MakeObstacle(new Vector2(-size.x / 2, 0), new Vector2(1, size.y), true);
            MakeObstacle(new Vector2(size.x / 2, 0), new Vector2(1, size.y), true);
            MakeObstacle(new Vector2(0, -size.y / 2), new Vector2(size.x, 1), true);
            MakeObstacle(new Vector2(0, size.y / 2), new Vector2(size.x, 1), true);
            if (Settings.Obstacles != null)
                foreach (var obstacle in Settings.Obstacles) MakeObstacle(obstacle.Position, obstacle.Size, obstacle.Wall, obstacle.Rotation);
        }

        private void MakeObstacle(Vector2 position, Vector2 size, bool wall, float rotation = 0)
        {
            string layer = wall ? "MURO" : "OSTACOLO";
            var go = TestVisuals.Box(layer, position, size, wall ? new Color(.25f, .28f, .8f) : new Color(.65f, .05f, .12f), 1);
            go.transform.rotation = Quaternion.Euler(0, 0, rotation);
            go.layer = LayerMask.NameToLayer(layer);
            go.AddComponent<BoxCollider2D>().size = size;
            go.AddComponent<TestObstacle>().IsWall = wall;
        }

        private void CreatePlayer()
        {
            var selectedWeapon = Definition.SelectedWeapon;
            var go = TestVisuals.Box(selectedWeapon.PG.PlayerId, Settings.StartPosition, Vector2.one, Color.white, 3);
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
            Player.Initialize(selectedWeapon.PG);
            Player.Actor.RoundFinalDamage = true; // GDD: round only after final DEF mitigation.
            Ability = null; Passive = null; PG02Ability = null; PG02Passive = null; PG03Ability = null; PG03Passive = null; PG04Ability = null; PG04Items = null; PG05Ability = null; PG06Ability = null; PG07Ability = null; PG08Ability = null; PG08Passive = null;
            cdReduction = Player.CdReduction;
            var muzzle = new GameObject("Bocca arma").transform;
            muzzle.SetParent(go.transform, false);
            muzzle.localPosition = new Vector3(.5f, 0, 0); // Existing test muzzle placement.
            var weapon = go.AddComponent<PlayerWeapon>();
            if (runtimeWeapon != null) Destroy(runtimeWeapon);
            runtimeWeapon = Instantiate(selectedWeapon);
            if (Definition.SelectedPlayer == LoopPlayer.PG01) runtimeWeapon.ConeAngle = 75;
            if (Definition.SelectedPlayer == LoopPlayer.PG05) { runtimeWeapon.Shape = AttackShape.Cone; runtimeWeapon.ConeAngle = 135; }
            weapon.Definition = runtimeWeapon;
            weapon.Muzzle = muzzle;
            if (Definition.SelectedPlayer == LoopPlayer.PG01)
            {
                Passive = go.AddComponent<PG01PassiveRuntime>();
                Passive.Initialize(Player.Actor, Definition.SelectedPassive);
                Ability = go.AddComponent<PG01AbilityRuntime>();
                Ability.Initialize(this, Definition.SelectedAbility, Definition.PG01Abilities);
                go.AddComponent<PG01AbilityInput>();
            }
            else if (Definition.SelectedPlayer == LoopPlayer.PG02)
            {
                PG02Passive = go.AddComponent<PG02PassiveRuntime>();
                PG02Passive.Initialize(this, Definition.SelectedPG02Passive);
                PG02Ability = go.AddComponent<PG02AbilityRuntime>();
                PG02Ability.Initialize(this, Definition.SelectedPG02Ability, Definition.PG02Abilities);
                go.AddComponent<PG02AbilityInput>();
            }
            else if (Definition.SelectedPlayer == LoopPlayer.PG03)
            {
                PG03Passive = go.AddComponent<PG03PassiveRuntime>();
                PG03Passive.Initialize(Definition.SelectedPG03Passive);
                PG03Ability = go.AddComponent<PG03AbilityRuntime>();
                PG03Ability.Initialize(this, Definition.SelectedPG03Ability, Definition.PG03Abilities);
                go.AddComponent<PG03AbilityInput>();
            }
            else if (Definition.SelectedPlayer == LoopPlayer.PG04)
            {
                PG04Items = go.AddComponent<PG04ItemSlots>();
                PG04Items.Initialize(Definition.PG04TestItemSlots, Definition.SelectedPG04Passive);
                PG04Ability = go.AddComponent<PG04AbilityRuntime>();
                PG04Ability.Initialize(this, Definition.SelectedPG04Ability, Definition.SelectedPG04Passive, Definition.PG04Abilities);
                go.AddComponent<PG04AbilityInput>();
            }
            else if (Definition.SelectedPlayer == LoopPlayer.PG05)
            {
                PG05Ability = go.AddComponent<PG05AbilityRuntime>();
                PG05Ability.Initialize(this, Definition.SelectedPG05Ability, Definition.SelectedPG05Passive, Definition.PG05Abilities);
                go.AddComponent<PG05AbilityInput>();
            }
            else if (Definition.SelectedPlayer == LoopPlayer.PG06)
            {
                PG06Ability = go.AddComponent<PG06AbilityRuntime>();
                PG06Ability.Initialize(this, Definition.SelectedPG06Ability, Definition.SelectedPG06Passive, Definition.PG06Abilities);
                go.AddComponent<PG06AbilityInput>();
            }
            else if (Definition.SelectedPlayer == LoopPlayer.PG07)
            {
                PG07Ability = go.AddComponent<PG07AbilityRuntime>();
                PG07Ability.Initialize(this, Definition.SelectedPG07Ability, Definition.SelectedPG07Passive, Definition.PG07Abilities);
                go.AddComponent<PG07AbilityInput>();
            }
            else
            {
                PG08Passive = go.AddComponent<PG08PassiveRuntime>();
                PG08Passive.Initialize(this, Definition.SelectedPG08Passive, Definition.PG08Abilities);
                PG08Ability = go.AddComponent<PG08AbilityRuntime>();
                PG08Ability.Initialize(this, Definition.SelectedPG08Ability, Definition.PG08Abilities);
                go.AddComponent<PG08AbilityInput>();
            }
            Bonuses = go.AddComponent<BonusInventory>();
            Bonuses.Catalog = Definition.BonusCatalog != null ? Definition.BonusCatalog : Settings.BonusCatalog;
            Bonuses.ApplyStatFallback = index => ApplyStat((AreaStat)index);
            BonusAbilities = go.AddComponent<BonusAbilityRuntime>();
            BonusAbilities.Initialize(this, Bonuses);
            Experience = go.AddComponent<ExperienceProgression>();
            Experience.Thresholds = (int[])Settings.ExperienceThresholds.Clone();
            go.SetActive(true);
        }

        public Combatant CreateMob(int index, Vector2 position)
        {
            var go = TestVisuals.Circle("ZOMB01", position, Settings.ActorRadius, new Color(.4f, .7f, .4f), 2);
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
            AbilityChoices = Bonuses.Generate(2, System.Array.ConvertAll(Choices, stat => (int)stat));
            Selected = -1;
        }

        public void SelectBonus(int index)
        {
            if (State == LoopState.Bonus && Choices != null && index >= 0 && index < Choices.Length + AbilityChoices.Length) Selected = index;
        }

        public void ConfirmBonus()
        {
            if (State != LoopState.Bonus || Selected < 0) return;
            if (Selected < Choices.Length) ApplyStat(Choices[Selected]);
            else Bonuses.Apply(AbilityChoices[Selected - Choices.Length]);
            AbilityChoices = null;
            Choices = null;
            Selected = -1;
            if (AreaIndex == Definition.AreaTotals.Length - 1)
            { State = LoopState.Finished; return; } // Test boundary; not a city/run victory.
            State = LoopState.Transition; // Guard double confirmation before yielding.
            StartCoroutine(NextArea());
        }

        private IEnumerator NextArea()
        {
            BonusAbilities?.ChangeArea();
            if (Ability != null) Ability.ChangeArea();
            if (PG02Ability != null) PG02Ability.ChangeArea();
            if (PG03Ability != null) PG03Ability.ChangeArea();
            if (PG04Ability != null) PG04Ability.ChangeArea();
            if (PG05Ability != null) PG05Ability.ChangeArea();
            if (PG06Ability != null) PG06Ability.ChangeArea();
            if (PG07Ability != null) PG07Ability.ChangeArea();
            if (PG02Passive != null) PG02Passive.ChangeArea();
            if (PG08Ability != null) PG08Ability.ChangeArea();
            if (PG08Passive != null) PG08Passive.ChangeArea();
            Exit.Entered -= OpenBonus;
            areaRoot.gameObject.SetActive(false);
            Destroy(areaRoot.gameObject);
            Destroy(Settings);
            yield return null; // Dispose corpse timers, effects, NavMesh and spawn subscriptions.
            Player.Actor.Heal(.15f);
            Experience.DropPercent = Experience.DropPercent * 105 / 100;
            AreaIndex++;
            yield return BuildArea();
        }

        private void ApplyStat(AreaStat stat) => AreaStatBonus.Apply(Player.Actor, stat, ref cdReduction);

        public void AddGold(int value) => Gold += value;
        public void RefreshNavigation()
        {
            if (Navigation != null && Settings != null) Navigation.Build(Settings.AreaSize);
        }
        public void RestartTest()
        {
            if (Loading) return;
            string issue = Definition == null ? "LoopDefinition mancante." : Definition.Validate();
            if (issue != null) { Fail(issue); return; }
            Experience?.CancelChoices();
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
            Player = null; Bonuses = null; BonusAbilities = null; Experience = null; AbilityChoices = null;
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
