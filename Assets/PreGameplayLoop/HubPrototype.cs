using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
namespace RogZombie.PreGameplayLoop
{
    public sealed class HubPrototype : MonoBehaviour
    {
        public LoopDefinition Definition;
        public TextAsset Content;
        public RunPreparationSelection Selection { get; private set; }
        public PreparationContent Text { get; private set; }
        public LoopSession Run { get; private set; }
        public Vector2 HubPosition { get; private set; }
        public readonly Vector2 ExitPosition = new Vector2(6, 0);
        public bool AtExit => Vector2.Distance(HubPosition, ExitPosition) <= 1;
        public string Failure { get; private set; }
        private LoopDefinition runDefinition;
        private Vector3 cameraPosition;
        private float cameraSize, previousTimeScale;
        private void Awake()
        {
            previousTimeScale = Time.timeScale; Time.timeScale = 1;
            Selection = new RunPreparationSelection();
            Text = Content != null ? JsonUtility.FromJson<PreparationContent>(Content.text) : null;
            if (Definition == null || Text == null || Text.Characters == null || Text.Characters.Length != 8)
                Failure = "Assegnare configurazione e testi del roster.";
            if (Camera.main != null) { cameraPosition = Camera.main.transform.position; cameraSize = Camera.main.orthographicSize; }
            gameObject.AddComponent<RunPreparationUI>();
        }
        private void Update()
        {
            if (Selection.Page != PreparationPage.Hub || Keyboard.current == null || !Application.isFocused) return;
            var k = Keyboard.current;
            Vector2 axis = new Vector2((k.dKey.isPressed ? 1 : 0) - (k.aKey.isPressed ? 1 : 0), (k.wKey.isPressed ? 1 : 0) - (k.sKey.isPressed ? 1 : 0));
            HubPosition += Vector2.ClampMagnitude(axis, 1) * 2 * Time.unscaledDeltaTime;
            HubPosition = new Vector2(Mathf.Clamp(HubPosition.x, -8, 8), Mathf.Clamp(HubPosition.y, -4, 4));
            if (AtExit && k.fKey.wasPressedThisFrame) Selection.OpenPreparation();
        }
        public bool BeginRun()
        {
            if (!Selection.CanStart || Run != null || Failure != null) return false;
            var definition = Selection.CreateRunDefinition(Definition);
            string issue = definition.Validate();
            if (issue != null) { Failure = issue; Destroy(definition); return false; }
            runDefinition = definition; Selection.StartRun();
            var go = new GameObject("RUN dal HUB"); go.transform.SetParent(transform);
            Run = go.AddComponent<LoopSession>(); Run.Definition = runDefinition; return true;
        }
        public void ReturnFromRun()
        {
            if (Run == null || (Run.State != LoopState.Finished && Run.State != LoopState.Defeat)) return;
            StartCoroutine(ReturnRoutine());
        }
        private IEnumerator ReturnRoutine()
        {
            Run.gameObject.SetActive(false); Destroy(Run.gameObject); Run = null;
            if (runDefinition != null) Destroy(runDefinition);
            yield return null;
            Time.timeScale = 1;
            if (Camera.main != null) { Camera.main.transform.position = cameraPosition; Camera.main.orthographicSize = cameraSize; }
            HubPosition = Vector2.zero; Selection.ReturnToHub();
        }
        private void OnDestroy()
        { if (runDefinition != null) Destroy(runDefinition); Time.timeScale = previousTimeScale; }
    }
}
