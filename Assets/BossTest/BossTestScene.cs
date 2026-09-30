using System.Collections;
using UnityEngine;
using RogZombie.TestEngine;
using RogZombie.PreGameplayLoop;
namespace RogZombie.BossTest
{
    [DefaultExecutionOrder(-200)]
    public sealed class BossTestScene : MonoBehaviour
    {
        public enum EntrancePhase { Area3, EntranceHold, CameraToBoss, BossPresentation, CameraZoomOut, Combat }
        public BossDefinition Boss;
        public bool StartAtCompletedArea3 = true;
        [Tooltip("Posizioni provvisorie per il test, non level design definitivo.")]
        public Vector2 PlayerStart = new Vector2(0, -12), BossStart = new Vector2(0, 8);
        [Tooltip("Offset dello spawn di prova rispetto all'uscita originale di AREA 3.")]
        public Vector2 Area3ApproachOffset = new Vector2(0, -10);
        [Header("Presentazione provvisoria approvata per il test")]
        [Min(0)] public float EntranceHoldSeconds = 1;
        [Range(0, .25f), Tooltip("Altezza di ciascuna bandella rispetto allo schermo.")] public float LetterboxHeight = .1f;
        private float letterboxAmount;
        [Min(0)] public float CameraPanSeconds = 1.5f;
        [Min(0)] public float PresentationSeconds = 2;
        [Min(0)] public float CameraZoomSeconds = 2;
        public BossBrain Runtime { get; private set; }
        public EntrancePhase Phase { get; private set; } = EntrancePhase.Area3;
        public bool InBossArea => loop != null && loop.AreaIndex == 3;
        public bool Presenting => InBossArea && Phase != EntrancePhase.Combat;
        public string PhaseLabel => Phase == EntrancePhase.Area3 ? "AREA 3 completata — raggiungi l'uscita con il gruppo" :
            Phase == EntrancePhase.EntranceHold ? "Ingresso — camera ferma sui PG" :
            Phase == EntrancePhase.CameraToBoss ? "Ingresso — camera verso il BOSS" :
            Phase == EntrancePhase.BossPresentation ? "Presentazione BOSS" :
            Phase == EntrancePhase.CameraZoomOut ? "Allontanamento camera — PG fermi all'ingresso" : "BOSS FIGHT";
        private LoopSession loop;
        private LoopDefinition runtimeDefinition;
        private TestAreaSettings geometry, approachGeometry;
        private Transform bossVisual;
        private Combatant victoryActor;
        private bool bossDefeated;
        private GUIStyle victoryTitle, victorySubtitle;

        private void OnBossDefeated(Combatant actor) { bossDefeated = true; }

        private void ClearVictory()
        {
            if (victoryActor != null) victoryActor.Died -= OnBossDefeated;
            victoryActor = null;
            bossDefeated = false;
        }

        private void DrawVictoryBanner()
        {
            if (victoryTitle == null)
            {
                victoryTitle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
                victoryTitle.normal.textColor = new Color(1f, .84f, .3f);
                victorySubtitle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
                victorySubtitle.normal.textColor = Color.white;
            }
            float scale = Mathf.Min(Screen.width / 960f, Screen.height / 540f);
            float width = 620 * scale, height = 112 * scale;
            var rect = new Rect((Screen.width - width) / 2, Screen.height * .28f, width, height);
            int previousDepth = GUI.depth;
            Color previousColor = GUI.color;
            GUI.depth = -90;
            GUI.color = new Color(.04f, .06f, .09f, .94f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = new Color(1f, .84f, .3f);
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 3 * scale), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(rect.x, rect.yMax - 3 * scale, rect.width, 3 * scale), Texture2D.whiteTexture);
            GUI.color = Color.white;
            victoryTitle.fontSize = Mathf.Max(12, Mathf.RoundToInt(38 * scale));
            victorySubtitle.fontSize = Mathf.Max(10, Mathf.RoundToInt(20 * scale));
            GUI.Label(new Rect(rect.x, rect.y + 12 * scale, width, 52 * scale), "VITTORIA", victoryTitle);
            GUI.Label(new Rect(rect.x, rect.y + 66 * scale, width, 30 * scale), "BOSS SCONFITTO", victorySubtitle);
            GUI.color = previousColor;
            GUI.depth = previousDepth;
        }

        private void Awake()
        {
            loop = GetComponent<LoopSession>();
            if (Boss == null || loop == null || loop.Definition == null)
            { Debug.LogError("Configurazione BOSS test mancante.", this); enabled = false; return; }
            runtimeDefinition = Instantiate(loop.Definition);
            loop.Definition = runtimeDefinition;
            if (StartAtCompletedArea3)
            {
            runtimeDefinition.SelectedPlayer = LoopPlayer.PG01;
            runtimeDefinition.EnableCompanion = true;
            runtimeDefinition.CompanionPlayer = LoopPlayer.PG02;
            runtimeDefinition.AdditionalCompanions = new[] {
                new CompanionSelection { Enabled = true, Player = LoopPlayer.PG03 },
                new CompanionSelection { Enabled = true, Player = LoopPlayer.PG04 } };

            }
            Vector2 approachExit = runtimeDefinition.ExitForArea(2);
            approachGeometry = Instantiate(runtimeDefinition.GeometryForArea(2));
            if (StartAtCompletedArea3) approachGeometry.StartPosition = approachExit + Area3ApproachOffset;
            approachGeometry.ForceTestChestNearPlayer = false;
            geometry = Instantiate(runtimeDefinition.GeometryTemplate);
            geometry.AreaSize = Boss.ArenaSize;
            geometry.StartPosition = PlayerStart;
            geometry.Obstacles = new ObstaclePlacement[0];
            geometry.ForceTestChestNearPlayer = false;
            var layouts = new System.Collections.Generic.List<AreaLayoutOverride>();
            if (runtimeDefinition.AreaLayouts != null)
                foreach (var layout in runtimeDefinition.AreaLayouts)
                    if (layout != null && layout.AreaNumber != 4 && (!StartAtCompletedArea3 || layout.AreaNumber != 3)) layouts.Add(layout);
            if (StartAtCompletedArea3)
                layouts.Add(new AreaLayoutOverride { AreaNumber = 3, Geometry = approachGeometry, ExitPosition = approachExit });
            layouts.Add(new AreaLayoutOverride { AreaNumber = 4, Geometry = geometry, ExitPosition = PlayerStart });
            runtimeDefinition.AreaLayouts = layouts.ToArray();
            // Shared loop validation expects population metadata for each index. Copy the
            // existing last entry; the BOSS area bypasses SpawnManager; the shortcut also skips AREA 3 mobs.
            int population = runtimeDefinition.AreaTotals[2];
            var distribution = runtimeDefinition.AreaMobDistributions[2];
            System.Array.Resize(ref runtimeDefinition.AreaTotals, 4);
            System.Array.Resize(ref runtimeDefinition.AreaMobDistributions, 4);
            runtimeDefinition.AreaTotals[3] = population;
            runtimeDefinition.BossAreaNumbers = new[] { 4 };
            runtimeDefinition.AreaMobDistributions[3] = distribution;
        }

        public void ResetEntrance()
        {
            ClearVictory();
            Phase = EntrancePhase.Area3;
            letterboxAmount = 0;
            Runtime = null;
            bossVisual = null;
        }

        public void Begin(Transform root)
        {
            ClearVictory();
            var go = TestVisuals.Circle("BOSS 01", BossStart, Boss.Diameter / 2, new Color(.6f, .18f, .16f), 3);
            go.transform.SetParent(root);
            go.layer = LayerMask.NameToLayer("MOB");
            go.AddComponent<CircleCollider2D>().radius = Boss.Diameter / 2;
            var actor = go.AddComponent<Combatant>(); actor.Initialize(Faction.MOB, Boss.Stats); actor.RoundFinalDamage = true;
            victoryActor = actor; victoryActor.Died += OnBossDefeated;
            var numbers = root.gameObject.AddComponent<DamageNumbersDebug>(); numbers.Settings = loop.Settings; numbers.ViewCamera = loop.GameCamera;
            Runtime = go.AddComponent<BossBrain>(); Runtime.Initialize(loop, Boss);
            // Animate only the artwork: the collider and all gameplay transforms stay fixed.
            var original = go.GetComponent<SpriteRenderer>();
            bossVisual = new GameObject("BOSS visual placeholder").transform;
            bossVisual.SetParent(go.transform, false);
            var renderer = bossVisual.gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = original.sprite; renderer.color = original.color; renderer.sortingOrder = original.sortingOrder;
            original.enabled = false;
            foreach (var member in loop.Members)
                if (member.Player.GetComponent<BossPlayerStatus>() == null) member.Player.gameObject.AddComponent<BossPlayerStatus>();
        }

        public IEnumerator PlayEntrance()
        {
            var camera = loop.GameCamera;
            camera.orthographic = false;
            camera.transform.position = new Vector3(loop.Controlled.Player.transform.position.x, loop.Controlled.Player.transform.position.y, -TopDownEnvironment.CameraHeight);
            Phase = EntrancePhase.EntranceHold;
            letterboxAmount = 1;
            if (EntranceHoldSeconds > 0) yield return new WaitForSecondsRealtime(EntranceHoldSeconds);
            Phase = EntrancePhase.CameraToBoss;
            yield return MoveCamera(new Vector3(BossStart.x, BossStart.y, -TopDownEnvironment.CameraHeight), CameraPanSeconds);
            Phase = EntrancePhase.BossPresentation;
            float elapsed = 0;
            while (elapsed < PresentationSeconds)
            {
                // Purely visual roar; the collider is not scaled and no attack is emitted.
                float pulse = Mathf.Sin(Mathf.PI * Mathf.Clamp01(elapsed / PresentationSeconds));
                bossVisual.localScale = Vector3.one * (1 + .12f * pulse);
                yield return null;
                elapsed += Time.unscaledDeltaTime;
            }
            bossVisual.localScale = Vector3.one;
            Phase = EntrancePhase.CameraZoomOut;
            yield return MoveCamera(new Vector3(0, 0, -FightCameraDistance()), CameraZoomSeconds, true);
            Runtime.StartFightTimer();
            Phase = EntrancePhase.Combat;
        }

        private IEnumerator MoveCamera(Vector3 destination, float seconds, bool removeLetterbox = false)
        {
            var camera = loop.GameCamera;
            Vector3 start = camera.transform.position;
            float elapsed = 0;
            while (elapsed < seconds)
            {
                float t = Mathf.SmoothStep(0, 1, elapsed / seconds);
                camera.transform.position = Vector3.Lerp(start, destination, t);
                if (removeLetterbox) letterboxAmount = 1 - t;
                yield return null;
                elapsed += Time.unscaledDeltaTime;
            }
            camera.transform.position = destination;
            if (removeLetterbox) letterboxAmount = 0;
        }

        private float FightCameraDistance()
        {
            var camera = loop.GameCamera;
            return Boss.CameraWidth / (2 * camera.aspect * Mathf.Tan(camera.fieldOfView * .5f * Mathf.Deg2Rad));
        }

        public void FollowCamera()
        {
            if (loop.Player == null || loop.Controlled == null) return;
            var camera = loop.GameCamera;
            if (Phase == EntrancePhase.Area3)
            {
                camera.orthographic = false;
                var pgPosition = loop.Controlled.Player.transform.position;
                camera.transform.position = new Vector3(pgPosition.x, pgPosition.y, -TopDownEnvironment.CameraHeight);
                return;
            }
            if (Phase != EntrancePhase.Combat) return; // Entrance coroutine owns the camera.
            float halfWidth = Boss.CameraWidth / 2, halfHeight = halfWidth / camera.aspect;
            camera.orthographic = false;
            Vector2 current = camera.transform.position, pg = loop.Controlled.Player.transform.position;
            Vector2 delta = pg - current;
            Vector2 shift = new Vector2(Mathf.Sign(delta.x) * Mathf.Max(0, Mathf.Abs(delta.x) - halfWidth + Boss.CameraBorder),
                Mathf.Sign(delta.y) * Mathf.Max(0, Mathf.Abs(delta.y) - halfHeight + Boss.CameraBorder));
            current += Vector2.ClampMagnitude(shift, loop.Controlled.Player.Actor.MovementMetresPerSecond * Time.deltaTime);
            float x = Mathf.Max(0, Boss.ArenaSize.x / 2 - halfWidth), y = Mathf.Max(0, Boss.ArenaSize.y / 2 - halfHeight);
            camera.transform.position = new Vector3(Mathf.Clamp(current.x, -x, x), Mathf.Clamp(current.y, -y, y), -FightCameraDistance());
        }

        private void OnGUI()
        {
            if (!StartAtCompletedArea3 && !InBossArea) return;
            if (Presenting)
            {
                // Screen overlays only: no camera aspect or world-space framing changes.
                int previousDepth = GUI.depth;
                Color previousColor = GUI.color;
                GUI.depth = -100;
                GUI.color = Color.black;
                float height = Screen.height * LetterboxHeight * letterboxAmount;
                GUI.DrawTexture(new Rect(0, 0, Screen.width, height), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(0, Screen.height - height, Screen.width, height), Texture2D.whiteTexture);
                GUI.color = previousColor;
                GUI.depth = previousDepth;
                return;
            }
            if (bossDefeated) { DrawVictoryBanner(); return; }
            string detail = Runtime != null && Phase == EntrancePhase.Combat
                ? $"HP {Runtime.Actor.CurrentHP:0}/{Boss.Stats.HP:0} — {Runtime.Status} | Prossimo speciale: {Runtime.SpecialRemaining:0.0}s"
                : "Simulazione AREA 3 → AREA BOSS";
            GUI.Box(new Rect(8, 110, Screen.width - 16, 54), $"{PhaseLabel}\n{detail}");
        }
        private void OnDestroy()
        {
            ClearVictory();
            if (runtimeDefinition != null) Destroy(runtimeDefinition);
            if (geometry != null) Destroy(geometry);
            if (approachGeometry != null) Destroy(approachGeometry);
        }
    }
}
