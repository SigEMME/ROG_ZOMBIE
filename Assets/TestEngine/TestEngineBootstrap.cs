using System.Collections;
using UnityEngine;

namespace RogZombie.TestEngine
{
    [DefaultExecutionOrder(-100)]
    public sealed class TestEngineBootstrap : MonoBehaviour, ISpawnWorld
    {
        public TestAreaSettings Settings;
        [Range(0, 7), Tooltip("0 = PG01, 7 = PG08. Changing PG restarts the test RUN.")]
        public int StartingPG;
        public bool ShowDebug;
        public Camera GameCamera { get; private set; }
        public PlayerRuntime Player { get; private set; }
        public SpawnManager Spawns { get; private set; }
        public TestNavigation Navigation { get; private set; }
        public bool Loading { get; private set; }
        public int Gold { get; private set; }
        public string Failure { get; private set; }
        private Transform runtimeRoot;
        private bool restarting;

        private void Start()
        {
            GameCamera = Camera.main;
            if (GetComponent<TestHUD>() == null) gameObject.AddComponent<TestHUD>();
            StartCoroutine(Build());
        }

        public void Restart(int index)
        {
            if (restarting || Loading) return;
            StartingPG = index;
            StartCoroutine(RestartRoutine());
        }

        private IEnumerator RestartRoutine()
        {
            restarting = true;
            Loading = true;
            Time.timeScale = 0f;
            if (runtimeRoot != null) Destroy(runtimeRoot.gameObject);
            yield return null;
            Gold = 0;
            yield return Build();
            restarting = false;
        }

        private IEnumerator Build()
        {
            Loading = true;
            Time.timeScale = 0f;
            Failure = ValidateConfiguration();
            if (Failure != null) { Debug.LogError(Failure, this); yield break; }
            GameCamera.orthographic = true;
            GameCamera.orthographicSize = Settings.CameraSize;
            GameCamera.backgroundColor = new Color(0.055f, 0.07f, 0.08f);
            runtimeRoot = new GameObject("TEST ENGINE #2 runtime").transform;
            TestVisuals.Root = runtimeRoot;
            var damageDebug = runtimeRoot.gameObject.AddComponent<DamageNumbersDebug>();
            damageDebug.Settings = Settings;
            damageDebug.ViewCamera = GameCamera;
            runtimeRoot.gameObject.AddComponent<HealingNumbers>().ViewCamera = GameCamera;
            BuildGeometry();
            Navigation = runtimeRoot.gameObject.AddComponent<TestNavigation>();
            Navigation.AgentRadius = Settings.ActorRadius;
            Navigation.ShowNavMesh = ShowDebug;
            Navigation.Build(Settings.AreaSize);
            if (!Navigation.Ready) { Failure = "NavMesh non disponibile: controllare la Console."; yield break; }
            CreatePlayer();
            FollowCamera();
            Spawns = runtimeRoot.gameObject.AddComponent<SpawnManager>();
            Spawns.ShowSpawnPoints = ShowDebug;
            Spawns.Initialize(Settings, Navigation, this);
            int kits = Random.Range(1, 4);
            for (int i = 0; i < kits; i++) yield return PlacePickup(false);
            if (Settings.ForceTestChestNearPlayer || Random.value * 100f < Settings.ChestRate) yield return PlacePickup(true);
            while (!Spawns.InitialComplete) yield return null;
            Loading = false;
            Time.timeScale = 1f;
        }

        private string ValidateConfiguration()
        {
            if (Settings == null || GameCamera == null) return "Assegnare TestAreaSettings e una Main Camera.";
            if (Settings.Weapons == null || Settings.Weapons.Length != 8 || Settings.Mobs == null || Settings.Mobs.Length != 5)
                return "Servono gli 8 PG e i 5 MOB configurati.";
            foreach (var weapon in Settings.Weapons) if (weapon == null || weapon.PG == null) return "Asset arma/PG mancante.";
            foreach (var mob in Settings.Mobs) if (mob == null) return "Asset MOB mancante.";
            if (Settings.BonusCatalog == null || Settings.BonusCatalog.Bonuses.Length < 3) return "Catalogo BONUS mancante o incompleto.";
            if (Settings.TotalMobs <= 0 || Settings.FirstSpawnPercent <= 0f || Settings.FirstSpawnPercent > 100f) return "Quantità SPAWN non valide.";
            if (Settings.MobPercentages == null || Settings.MobPercentages.Length != 5) return "Configurare le cinque percentuali MOB.";
            float sum = 0f;
            foreach (float weight in Settings.MobPercentages) { if (weight < 0f) return "Percentuale MOB negativa."; sum += weight; }
            if (Mathf.Abs(sum - 100f) > 0.001f) return "Le percentuali MOB devono sommare a 100.";
            if (Settings.SearchAttemptsPerFrame < 1 || Settings.OffscreenExtraRange <= 0f) return "Parametri ricerca SPAWN non validi.";
            if (Settings.AreaSize.x <= 0f || Settings.AreaSize.y <= 0f || Settings.CameraSize <= 0f || Settings.ActorRadius <= 0f) return "Geometria AREA non valida.";
            if (Settings.ExperienceThresholds == null || Settings.ExperienceThresholds.Length == 0) return "Tabella EXP mancante.";
            foreach (int threshold in Settings.ExperienceThresholds) if (threshold <= 0) return "Soglia EXP non positiva.";
            return null;
        }

        private void BuildGeometry()
        {
            Vector2 size = Settings.AreaSize;
            TestVisuals.Box("Test floor", Vector2.zero, size, new Color(0.09f, 0.12f, 0.13f), -10);
            TestVisuals.FloorGrid(size);
            MakeObstacle(new Vector2(-size.x * 0.5f, 0), new Vector2(1f, size.y), true);
            MakeObstacle(new Vector2(size.x * 0.5f, 0), new Vector2(1f, size.y), true);
            MakeObstacle(new Vector2(0, -size.y * 0.5f), new Vector2(size.x, 1f), true);
            MakeObstacle(new Vector2(0, size.y * 0.5f), new Vector2(size.x, 1f), true);
            if (Settings.Obstacles != null)
                foreach (var item in Settings.Obstacles) MakeObstacle(item.Position, item.Size, item.Wall, item.Rotation);
        }

        private void MakeObstacle(Vector2 position, Vector2 size, bool wall, float rotation = 0)
        {
            var go = TestVisuals.Box(wall ? "MURO" : "OSTACOLO", position, size,
                wall ? new Color(0.35f, 0.4f, 0.43f) : new Color(0.45f, 0.3f, 0.18f), 1);
            go.transform.rotation = Quaternion.Euler(0, 0, rotation);
            go.AddComponent<BoxCollider2D>().size = size;
            go.AddComponent<TestObstacle>().IsWall = wall;
        }

        private void CreatePlayer()
        {
            var weapon = Settings.Weapons[StartingPG];
            var go = TestVisuals.Box(weapon.PG.PlayerId, Settings.StartPosition, Vector2.one, Color.white, 3);
            go.SetActive(false);
            if (Settings.PlayerSprite != null)
            {
                var sprite = go.GetComponent<SpriteRenderer>();
                sprite.drawMode = SpriteDrawMode.Simple;
                sprite.sprite = Settings.PlayerSprite;
            }
            go.AddComponent<CircleCollider2D>().radius = Settings.ActorRadius;
            go.AddComponent<Combatant>();
            go.AddComponent<PlayerMovement>().TestSettings = Settings;
            go.AddComponent<PlayerAim>();
            Player = go.AddComponent<PlayerRuntime>();
            Player.Initialize(weapon.PG);
            var muzzle = new GameObject("Bocca arma").transform;
            muzzle.SetParent(go.transform, false);
            muzzle.localPosition = new Vector3(0.5f, 0f, 0f);
            var firing = go.AddComponent<PlayerWeapon>();
            firing.Definition = weapon;
            firing.Muzzle = muzzle;
            firing.ShowDebug = ShowDebug;
            go.AddComponent<BonusInventory>().Catalog = Settings.BonusCatalog;
            go.AddComponent<ExperienceProgression>().Thresholds = (int[])Settings.ExperienceThresholds.Clone();
            go.SetActive(true);
        }

        public Combatant CreateMob(int index, Vector2 position)
        {
            Color[] colors = { new Color(0.4f, 0.7f, 0.4f), new Color(0.6f, 0.8f, 0.25f),
                new Color(0.65f, 0.35f, 0.8f), new Color(1f, 0.45f, 0.55f), new Color(1f, 0.5f, 0.15f) };
            var go = TestVisuals.Circle(Settings.Mobs[index].Kind.ToString(), position, Settings.ActorRadius, colors[index], 2);
            go.AddComponent<CircleCollider2D>().radius = Settings.ActorRadius;
            var actor = go.AddComponent<Combatant>();
            var brain = go.AddComponent<MobBrain>();
            brain.ShowAggro = ShowDebug;
            brain.ShowAttackArea = ShowDebug;
            brain.Initialize(Settings.Mobs[index], Navigation);
            go.AddComponent<MobSeparation>().Settings = Settings;
            return actor;
        }

        private IEnumerator PlacePickup(bool chest)
        {
            while (true)
            {
                for (int i = 0; i < Settings.SearchAttemptsPerFrame; i++)
                {
                    var point = new Vector2(Random.Range(-Settings.AreaSize.x * 0.5f, Settings.AreaSize.x * 0.5f),
                        Random.Range(-Settings.AreaSize.y * 0.5f, Settings.AreaSize.y * 0.5f));
                    bool nearTestChest = chest && Settings.ForceTestChestNearPlayer;
                    if (nearTestChest) point = Settings.StartPosition + Settings.TestChestOffset + Random.insideUnitCircle * 0.5f;
                    if (!Navigation.Sample(point, out point, 0.5f)) continue;
                    if (!nearTestChest && Vector2.Distance(point, Settings.StartPosition) < Settings.PickupMinStartDistance) continue;
                    if (nearTestChest && Vector2.Distance(point, Settings.StartPosition) <= Settings.ActorRadius + Settings.PickupTriggerRadius) continue;
                    if (!Navigation.Reachable(point, Settings.StartPosition)) continue;
                    var go = TestVisuals.Box(chest ? "CHEST" : "MEDI KIT", point, Vector2.one,
                        chest ? Color.yellow : new Color(0.2f, 1f, 0.85f), 2);
                    var trigger = go.AddComponent<CircleCollider2D>();
                    trigger.radius = Settings.PickupTriggerRadius;
                    trigger.isTrigger = true;
                    var pickup = go.AddComponent<AreaPickup>();
                    pickup.IsChest = chest;
                    pickup.TriggerRadius = Settings.PickupTriggerRadius;
                    pickup.HealPercent = Settings.MediKitHealPercent;
                    pickup.ChestWeights = Settings.ChestStatWeights;
                    pickup.ShowTrigger = ShowDebug;
                    yield break;
                }
                yield return null;
            }
        }

        private void LateUpdate() => FollowCamera();
        private void FollowCamera()
        {
            if (Player != null && GameCamera != null)
                GameCamera.transform.position = new Vector3(Player.transform.position.x, Player.transform.position.y, -10f);
        }
        public void AddGold(int value) => Gold += value;
        private void OnDestroy() => Time.timeScale = 1f;
    }
}
