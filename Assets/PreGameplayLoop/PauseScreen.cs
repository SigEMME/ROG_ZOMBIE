using UnityEngine;

namespace RogZombie.PreGameplayLoop
{
    public enum PausePage { Closed, Menu, Options, ReturnConfirmation, ExitConfirmation }

    // Local PLAYER menu. Network pause consensus belongs to the future multiplayer session.
    [DefaultExecutionOrder(-200)]
    public sealed class PauseScreen : MonoBehaviour
    {
        public Texture2D ResumeImage, OptionsImage, ReturnImage, ExitImage;
        public Texture2D ConfirmImage, CancelImage;
        public PausePage Page { get; private set; }
        public bool IsOpen => Page != PausePage.Closed;
        public bool IsHubPause { get; private set; }
        public const float BackgroundOpacity = .65f;
        private HubPrototype hub;
        private LoopSession pausedRun;
        private float savedTimeScale;

        private void Awake() => hub = GetComponent<HubPrototype>();
        private void Update()
        {
            if (IsOpen && (IsHubPause ? hub.Run != null || hub.Selection.Page != PreparationPage.Hub : hub.Run == null || hub.Run != pausedRun)) Resume();
        }
        public void Open()
        {
            if (IsOpen || hub.AtTitle || hub.Selection == null) return;
            if (hub.Run == null && hub.Selection.Page != PreparationPage.Hub) return;
            if (hub.Run != null && hub.Run.Loading) return;
            IsHubPause = hub.Run == null;
            pausedRun = hub.Run;
            savedTimeScale = Time.timeScale;
            Time.timeScale = 0;
            Page = PausePage.Menu;
        }
        public void Resume()
        {
            if (!IsOpen) return;
            Page = PausePage.Closed;
            Time.timeScale = savedTimeScale;
            pausedRun = null;
        }
        public void Back()
        {
            if (!IsOpen) Open();
            else if (Page == PausePage.Menu) Resume();
            else Page = PausePage.Menu;
        }
        public void ShowOptions() { if (Page == PausePage.Menu) Page = PausePage.Options; }
        public void RequestReturn() { if (Page == PausePage.Menu && !IsHubPause) Page = PausePage.ReturnConfirmation; }
        public void RequestExit() { if (Page == PausePage.Menu) Page = PausePage.ExitConfirmation; }
        public void Confirm()
        {
            if (Page != PausePage.ReturnConfirmation && Page != PausePage.ExitConfirmation) return;
            bool quit = Page == PausePage.ExitConfirmation;
            Resume();
            hub.AbandonRun();
            if (!quit) return;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        private void OnDisable() => Resume();

        private void OnGUI()
        {
            // Handle menu navigation as a GUI event, including while gameplay time is frozen.
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            { Back(); Event.current.Use(); }
            if (!IsOpen) return;
            var matrix = GUI.matrix;
            var color = GUI.color;
            int depth = GUI.depth;
            GUI.depth = -100;
            GUI.matrix = Matrix4x4.identity;
            GUI.color = new Color(.5f, .5f, .5f, BackgroundOpacity);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
            float scale = Mathf.Min(Screen.width / 1536f, Screen.height / 864f);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1536 * scale) / 2,
                (Screen.height - 864 * scale) / 2), Quaternion.identity, Vector3.one * scale);
            if (Page == PausePage.Menu)
            {
                float first = IsHubPause ? 182 : 107;
                if (ImageButton(first, ResumeImage, "RIPRENDI")) Resume();
                if (ImageButton(first + 150, OptionsImage, "OPZIONI")) ShowOptions();
                if (!IsHubPause && ImageButton(first + 300, ReturnImage, "TORNA ALL'HUB")) RequestReturn();
                if (ImageButton(first + (IsHubPause ? 300 : 450), ExitImage, "ESCI")) RequestExit();
            }
            else
            {
                var label = new GUIStyle(GUI.skin.label) { fontSize = 26, alignment = TextAnchor.MiddleCenter, wordWrap = true };
                GUI.Box(new Rect(358, 272, 820, 320), GUIContent.none);
                string message = Page == PausePage.Options ? "OPZIONI\nSchermata da definire" :
                    (Page == PausePage.ReturnConfirmation ? "TORNARE ALL'HUB?" : "USCIRE DAL GIOCO?") +
                    (IsHubPause ? "" : "\nPerderai tutti i progressi e i G ottenuti durante questa RUN.");
                GUI.Label(new Rect(388, 300, 760, 160), message, label);
                if (Page == PausePage.Options)
                { if (GUI.Button(new Rect(648, 490, 240, 60), "INDIETRO")) Back(); }
                else
                {
                    if (ImageButton(new Rect(398, 460, 360, 120), ConfirmImage, "CONFERMA")) Confirm();
                    if (ImageButton(new Rect(778, 460, 360, 120), CancelImage, "ANNULLA")) Back();
                }
            }
            GUI.matrix = matrix; GUI.color = color; GUI.depth = depth;
        }
        private static bool ImageButton(float y, Texture2D image, string label)
            => ImageButton(new Rect(468, y, 600, 200), image, label);

        private static bool ImageButton(Rect rect, Texture2D image, string label)
        {
            var hit = new Rect(rect.x + rect.width * .06f, rect.y + rect.height * .17f,
                rect.width * .88f, rect.height * .65f);
            GUI.SetNextControlName(label);
            bool click = GUI.Button(hit, image == null ? new GUIContent(label) : GUIContent.none,
                image == null ? GUI.skin.button : GUIStyle.none);
            var color = GUI.color;
            GUI.color = hit.Contains(Event.current.mousePosition) ? new Color(1, 1, .85f) : Color.white;
            if (image != null) GUI.DrawTexture(rect, image, ScaleMode.ScaleToFit, true);
            GUI.color = color;
            return click;
        }
    }
}
