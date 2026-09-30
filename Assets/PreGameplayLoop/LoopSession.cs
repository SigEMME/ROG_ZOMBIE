using System.Collections;
using UnityEngine;
using RogZombie.TestEngine;

namespace RogZombie.PreGameplayLoop
{
    public enum LoopState { Loading, Combat, AreaComplete, Bonus, Transition, Finished, Defeat, Error }

    [DefaultExecutionOrder(-100)]
    public sealed partial class LoopSession : MonoBehaviour, ISpawnWorld
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
        public bool PauseMenuOpen => GetComponentInParent<HubPrototype>()?.Pause?.IsOpen == true;
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
        public ItemRuntime Items { get; private set; }
        public SprintRuntime Sprint { get; private set; }
        public bool GameplayRunning => ParentSession != null ? ParentSession.GameplayRunning : Time.timeScale > 0 && (State == LoopState.Combat || State == LoopState.AreaComplete);
        public string Failure { get; private set; }
        public ResurrectionRuntime Resurrection { get; private set; }
        public BonusInventory Bonuses { get; private set; }
        public BonusAbilityRuntime BonusAbilities { get; private set; }
        public ExperienceProgression Experience { get; private set; }
        public BonusChoice[] AbilityChoices { get; private set; }
        public AreaStat[] Choices { get; private set; }
        public int Selected { get; private set; } = -1;
        private float cdReduction;
        private Transform areaRoot;
        private TopDownEnvironment environment;
        private float previousTimeScale;
        private WeaponDefinition runtimeWeapon;

        private void Start()
        {
            if (ParentSession != null) return;
            previousTimeScale = Time.timeScale;
            if (GetComponent<RogZombie.BossTest.BossTestScene>()?.StartAtCompletedArea3 == true) AreaIndex = 2;
            GameCamera = Camera.main;
            gameObject.AddComponent<LoopHUD>();
            string issue = Definition == null ? "LoopDefinition mancante." : Definition.Validate();
            foreach (string layer in new[] { "PG", "MOB", "MURO", "OSTACOLO", "TRIGGER_PG", "TRIGGER_MOB", "PET", "AREA_EFFECT_PG", "AREA_EFFECT_MOB" })
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
            Settings = Instantiate(Definition.GeometryForArea(AreaIndex));
            Settings.TotalMobs = Definition.AreaTotals[AreaIndex];
            Settings.FirstSpawnPercent = 30;
            Settings.MobPercentages = (float[])Definition.AreaMobDistributions[AreaIndex].Percentages.Clone();
            Settings.OffscreenExtraRange = 15;
            Settings.PlayerMoveSpeedBase = 2; // GDD unit conversion, supersedes old test tuning only here.
            Settings.Mobs = new MobDefinition[5];
            for (int i = 0; i < Settings.Mobs.Length; i++) Settings.Mobs[i] = Definition.MobAt(i);
            GameCamera.orthographic = false;
            GameCamera.orthographicSize = Settings.CameraSize;
            GameCamera.fieldOfView = 2 * Mathf.Atan(Settings.CameraSize / TopDownEnvironment.CameraHeight) * Mathf.Rad2Deg;
            GameCamera.transform.rotation = Quaternion.identity;
            areaRoot = new GameObject("AREA test " + (AreaIndex + 1)).transform;
            areaRoot.SetParent(transform);
            TestVisuals.Root = areaRoot;
            environment = areaRoot.gameObject.AddComponent<TopDownEnvironment>();
            environment.Initialize(Definition.EnvironmentShader);
            BuildGeometry();
            Navigation = areaRoot.gameObject.AddComponent<TestNavigation>();
            Navigation.AgentRadius = Settings.ActorRadius;
            Navigation.Build(Settings.AreaSize);
            if (!Navigation.Ready || !Navigation.Sample(Settings.StartPosition, out var start, .1f) ||
                !Navigation.Sample(Definition.ExitForArea(AreaIndex), out var exit, .1f) || !Navigation.Reachable(start, exit))
            { Fail("Inizio/uscita non validi o non collegati sulla NavMesh."); yield break; }
            if (Controlled == null) Controlled = this;
            if (Player == null) CreatePlayer();
            PrepareCompanion();
            if (State == LoopState.Error) yield break;
            Player.GetComponent<PlayerMovement>().TestSettings = Settings;
            Player.transform.SetPositionAndRotation(Settings.StartPosition, Quaternion.identity);
            PlaceCompanion();
            FollowCamera();
            var bossTest = GetComponent<RogZombie.BossTest.BossTestScene>();
            if (bossTest != null && bossTest.enabled && (bossTest.StartAtCompletedArea3 || bossTest.InBossArea))
            {
                yield return BuildBossTestStage(bossTest);
                yield break;
            }
            var exitObject = TestVisuals.Box("USCITA AREA", Definition.ExitForArea(AreaIndex), Vector2.one, Color.gray, 1);
            Exit = exitObject.AddComponent<AreaExitTrigger>();
            Exit.Initialize(Player.Actor);
            Exit.Entered += OpenBonus;
            Exit.PartyReady = PartyReadyForExit;
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
            Vector2 size = Settings.AreaSize; bool bossArena = GetComponent<RogZombie.BossTest.BossTestScene>()?.InBossArea ?? false; Vector2 wallHalf = size / 2 + (bossArena ? Vector2.one * .5f : Vector2.zero);
            environment.PrepareBuildings(Settings.Obstacles);
            environment.Floor(size, Settings.UrbanArea01);
            if (!Settings.UrbanArea01) TestVisuals.FloorGrid(size);
            TestVisuals.Box("SPAWN PG", Settings.StartPosition, Vector2.one, Color.green, -8);
            if (!bossArena) TestVisuals.Box("Riferimento USCITA", Definition.ExitForArea(AreaIndex), Vector2.one * 1.5f, Color.yellow, -8);
            MakeObstacle(new Vector2(-wallHalf.x, 0), new Vector2(1, size.y), true, boundary: true);
            MakeObstacle(new Vector2(wallHalf.x, 0), new Vector2(1, size.y), true, boundary: true);
            MakeObstacle(new Vector2(0, -wallHalf.y), new Vector2(size.x, 1), true, boundary: true);
            MakeObstacle(new Vector2(0, wallHalf.y), new Vector2(size.x, 1), true, boundary: true);
            if (Settings.Obstacles != null)
                foreach (var obstacle in Settings.Obstacles) MakeObstacle(obstacle.Position, obstacle.Size, obstacle.Wall, obstacle.Rotation);
        }

        private void MakeObstacle(Vector2 position, Vector2 size, bool wall, float rotation = 0, bool boundary = false)
        {
            string layer = wall ? "MURO" : "OSTACOLO";
            var go = TestVisuals.Box(layer, position, size, wall ? new Color(.25f, .28f, .8f) : new Color(.65f, .05f, .12f), 1);
            go.transform.rotation = Quaternion.Euler(0, 0, rotation);
            go.layer = LayerMask.NameToLayer(layer);
            go.AddComponent<BoxCollider2D>().size = size;
            go.AddComponent<TestObstacle>().IsWall = wall;
            go.GetComponent<SpriteRenderer>().enabled = false;
            environment.Obstacle(go.transform, size, wall, boundary);
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
            Resurrection = go.AddComponent<ResurrectionRuntime>();
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
            weapon.TrajectoryVisibleWhen = () => DirectlyControlled;
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
            PG04Items = go.AddComponent<PG04ItemSlots>();
            PG04Items.Initialize(ParentSession != null ? 0 : Definition.SelectedPlayer == LoopPlayer.PG04 ? Definition.PG04TestItemSlots : 4,
                Definition.SelectedPlayer == LoopPlayer.PG04 ? Definition.SelectedPG04Passive : PG04Passive.Pyromania);
            for (int slot = 0; slot < PG04Items.SlotCount; slot++)
                if (Definition.StartingItems != null && Definition.StartingItemCounts != null && slot < Definition.StartingItems.Length && slot < Definition.StartingItemCounts.Length)
                    for (int count = 0; count < Mathf.Min(3, Definition.StartingItemCounts[slot]); count++) PG04Items.TryAdd(slot, Definition.StartingItems[slot]);
            Items = go.AddComponent<ItemRuntime>(); Items.Initialize(this, PG04Items);
            Sprint = go.AddComponent<SprintRuntime>(); Sprint.Initialize(this);
            Bonuses = go.AddComponent<BonusInventory>();
            Bonuses.Catalog = Definition.BonusCatalog != null ? Definition.BonusCatalog : Settings.BonusCatalog;
            Bonuses.ApplyStatFallback = index => ApplyStat((AreaStat)index);
            BonusAbilities = go.AddComponent<BonusAbilityRuntime>();
            BonusAbilities.Initialize(this, Bonuses);
            Experience = go.AddComponent<ExperienceProgression>();
            Experience.Thresholds = (int[])Settings.ExperienceThresholds.Clone();
            go.AddComponent<PartyCommands>().Context = this;
            go.SetActive(true);
            if (ParentSession != null) go.GetComponent<PlayerMovement>().enabled = false;
        }

        public Combatant CreateMob(int index, Vector2 position)
        {
            var definition = Settings.Mobs[index];
            Color color = index == 0 ? new Color(.4f, .7f, .4f) : index == 1 ? new Color(1f, .55f, .15f) :
                index == 2 ? new Color(.7f, .35f, .9f) : index == 3 ? Color.cyan : new Color(.9f, .25f, .25f);
            var go = TestVisuals.Circle(definition.Kind.ToString(), position, Settings.ActorRadius, color, 2);
            go.layer = LayerMask.NameToLayer("MOB");
            go.AddComponent<CircleCollider2D>().radius = Settings.ActorRadius;
            var actor = go.AddComponent<Combatant>();
            // Growth counts ordinary AREAS only, without resetting at city boundaries.
            go.AddComponent<MobBrain>().Initialize(definition, Navigation, Definition.OrdinaryAreasBefore(AreaIndex));
            go.AddComponent<MobSeparation>().Settings = Settings;
            return actor;
        }

        private void Update()
        {
            if (ParentSession != null || PauseMenuOpen) return;
            if (State != LoopState.Combat && State != LoopState.AreaComplete) return;
            UpdatePartyControl();
            if (!Controlled.Player.Actor.IsActive)
            {
                State = LoopState.Defeat;
                Time.timeScale = 0; // No active party member remains.
                return;
            }
            TickResurrection(Time.deltaTime, UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.fKey.isPressed);
            if (State == LoopState.Combat && Spawns != null && Spawns.Killed == Settings.TotalMobs &&
                Spawns.TotalSpawned == Settings.TotalMobs && Spawns.Alive == 0)
            {
                State = LoopState.AreaComplete;
                Exit.Open();
            }
        }

        private void OpenBonus()
        {
            if (State != LoopState.AreaComplete || !Controlled.Player.Actor.IsActive) return;
            State = LoopState.Bonus;
            Time.timeScale = 0;
            rewardContext = this;
            foreach (var member in Members) if (member.Player.Actor.State != LifeState.Dead) { rewardContext = member; break; }
            DrawAreaChoices();
            Selected = -1;
        }

        public void SelectBonus(int index)
        {
            if (State == LoopState.Bonus && Choices != null && index >= 0 && index < Choices.Length + AbilityChoices.Length) Selected = index;
        }

        public void ConfirmBonus()
        {
            if (State != LoopState.Bonus || Selected < 0) return;
            if (Selected < Choices.Length) RewardContext.ApplyStat(Choices[Selected]);
            else RewardContext.Bonuses.Apply(AbilityChoices[Selected - Choices.Length]);
            bool passedCurrent = false;
            foreach (var member in Members)
            {
                if (member == RewardContext) { passedCurrent = true; continue; }
                if (passedCurrent && member.Player.Actor.State != LifeState.Dead)
                { rewardContext = member; DrawAreaChoices(); return; }
            }
            rewardContext = null;
            AbilityChoices = null;
            Choices = null;
            Selected = -1;
            if (AreaIndex == Definition.AreaTotals.Length - 1)
            { State = LoopState.Finished; return; } // Test boundary; not a city/run victory.
            State = LoopState.Transition; // Guard double confirmation before yielding.
            StartCoroutine(NextArea());
        }

        private void ClearAbilityEffects()
        {
            Items?.CancelAim();
            Sprint?.ChangeArea();
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
        }

        private IEnumerator NextArea()
        {
            foreach (var member in Members) member.ClearAbilityEffects();
            Exit.Entered -= OpenBonus;
            areaRoot.gameObject.SetActive(false);
            Destroy(areaRoot.gameObject);
            Destroy(Settings);
            yield return null; // Dispose corpse timers, effects, NavMesh and spawn subscriptions.
            foreach (var member in Members)
            { RestoreMemberForArea(member); member.Experience.DropPercent = member.Experience.DropPercent * 105 / 100; }
            AreaIndex++;
            yield return BuildArea();
        }

        private void ApplyStat(AreaStat stat) => AreaStatBonus.Apply(Player.Actor, stat, ref cdReduction);

        public void AddGold(int value) => Gold += value;
        public void DiscardRunProgress()
        {
            foreach (var member in Members) member.Gold = 0;
        }
        public void RefreshNavigation()
        {
            if (ParentSession != null) { ParentSession.RefreshNavigation(); return; }
            if (Navigation != null && Settings != null) Navigation.Build(Settings.AreaSize);
        }
        public void RestartTest()
        {
            if (Loading) return;
            string issue = Definition == null ? "LoopDefinition mancante." : Definition.Validate();
            if (issue != null) { Fail(issue); return; }
            foreach (var member in Members) member.Experience?.CancelChoices();
            StopAllCoroutines();
            State = LoopState.Transition;
            Time.timeScale = 0;
            StartCoroutine(RestartRoutine());
        }

        private IEnumerator RestartRoutine()
        {
            foreach (var member in companions) { member.gameObject.SetActive(false); Destroy(member.gameObject); }
            companions.Clear();
            Controlled = this; rewardContext = null;
            if (areaRoot != null) { areaRoot.gameObject.SetActive(false); Destroy(areaRoot.gameObject); }
            if (Player != null) { Player.gameObject.SetActive(false); Destroy(Player.gameObject); }
            if (Settings != null) Destroy(Settings);
            yield return null;
            Player = null; Bonuses = null; BonusAbilities = null; Experience = null; AbilityChoices = null;
            var bossTest = GetComponent<RogZombie.BossTest.BossTestScene>();
            bossTest?.ResetEntrance();
            AreaIndex = bossTest != null && bossTest.StartAtCompletedArea3 ? 2 : 0;
            Gold = 0;
            Choices = null;
            Selected = -1;
            Failure = null;
            yield return BuildArea();
        }
        private void LateUpdate() { if (ParentSession == null) FollowCamera(); }
        private void FollowCamera()
        {
            var bossTest = GetComponent<RogZombie.BossTest.BossTestScene>();
            if (bossTest != null && bossTest.enabled && (bossTest.StartAtCompletedArea3 || bossTest.InBossArea)) { bossTest.FollowCamera(); return; }
            if (Player != null && GameCamera != null)
                GameCamera.transform.position = new Vector3(Controlled.Player.transform.position.x, Controlled.Player.transform.position.y, -TopDownEnvironment.CameraHeight);
        }

        private void OnDestroy()
        {
            if (ParentSession != null) { if (runtimeWeapon != null) Destroy(runtimeWeapon); if (Definition != null) Destroy(Definition); return; }
            if (Exit != null) Exit.Entered -= OpenBonus;
            if (Settings != null) Destroy(Settings);
            if (TestVisuals.Root == areaRoot) TestVisuals.Root = null;
            if (runtimeWeapon != null) Destroy(runtimeWeapon);
            Time.timeScale = previousTimeScale;
        }
    }
}
