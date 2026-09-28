using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RogZombie.TopDown.Editor
{
    [InitializeOnLoad]
    public static class TopDownStudyValidation
    {
        private const string Key="ROG.TopDownStudy";
        private static IEnumerator checks;
        private static double deadline;
        private static readonly List<string> results=new List<string>();
        private static int warnings;
        static TopDownStudyValidation()
        {
            EditorApplication.update+=Tick;
            EditorApplication.playModeStateChanged+=state=>
            { if(state==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Key,false)) { results.Clear(); warnings=0; deadline=EditorApplication.timeSinceStartup+30; checks=Checks(); } };
            Application.logMessageReceived+=(message,stack,type)=>
            { if(checks==null) return; if(type==LogType.Warning) warnings++; if(type==LogType.Error || type==LogType.Exception) Finish(false,message+stack); };
        }
        [MenuItem("ROG ZOMBIE/Top-down/Open visual study")]
        public static void Open()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode || !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            const string path="Assets/Scenes/TopDownStudy.unity";
            if(File.Exists(path)) { EditorSceneManager.OpenScene(path); return; }
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var demo=new GameObject("Top-down visual study").AddComponent<TopDownStudy>();
            demo.StudyShader=AssetDatabase.LoadAssetAtPath<Shader>("Assets/TopDownStudy/TopDownStudy.shader");
            EditorSceneManager.SaveScene(scene,path);
        }
        private static void Tick()
        {
            const string request=".top-down-study-test.request";
            if(!EditorApplication.isCompiling && !EditorApplication.isPlayingOrWillChangePlaymode && File.Exists(request) && !SceneManager.GetActiveScene().isDirty)
            { SessionState.SetString(Key+"Report",File.ReadAllText(request).Trim()); File.Delete(request); Open(); SessionState.SetBool(Key,true); EditorApplication.isPlaying=true; }
            if(checks==null) return;
            try { if(EditorApplication.timeSinceStartup>deadline) throw new Exception("Timeout"); if(!checks.MoveNext()) Finish(true,"Play Mode left open for visual inspection; no gameplay conversion or build."); }
            catch(Exception ex) { Finish(false,ex.ToString()); }
        }
        private static void Check(bool ok,string text) { if(!ok) throw new Exception(text); results.Add("PASS "+text); }
        private static void Finish(bool ok,string detail)
        {
            checks=null; SessionState.SetBool(Key,false);
            File.WriteAllText(SessionState.GetString(Key+"Report","TOP-DOWN-validation.txt"),(ok?"PASS":"FAIL")+$" | Unity {Application.unityVersion} | checks={results.Count} | warnings={warnings}\n"+string.Join("\n",results)+"\n"+detail);
            if(!ok) EditorApplication.isPlaying=false;
        }
        private static IEnumerator Checks()
        {
            var demo=UnityEngine.Object.FindFirstObjectByType<TopDownStudy>();
            while(demo!=null && demo.View==null) yield return null;
            Check(demo!=null && demo.Player!=null,"Scene creates camera and controllable PG");
            Check(demo.StudyShader.isSupported && !ShaderUtil.ShaderHasError(demo.StudyShader),"Study shader supported without compilation errors");
            Check(demo.Perspective && demo.View.transform.forward==Vector3.forward,"Perspective camera faces ground vertically");
            Check(GameObject.Find("Building 5m")!=null && GameObject.Find("Building 10m")!=null,"Two buildings of distinct heights");
            Check(UnityEngine.Object.FindObjectsByType<BoxCollider2D>(FindObjectsSortMode.None).Length==2,"Two ground footprints");
            Check(demo.CanStand(new Vector2(0,3)) && !demo.CanStand(new Vector2(11,3)),"Road walkable and building interior blocked");
            demo.Player.position=new Vector3(0,3); demo.Move(new Vector2(20,0));
            Check(demo.Player.position.x<=5.66f,"Movement stops at footprint including PG radius");
            demo.Move(new Vector2(2,2));
            Check(demo.Player.position.x<=5.66f && demo.Player.position.y>4.9f,"Movement slides along facade");
            demo.ResetView(); var screen=demo.View.WorldToScreenPoint(Vector3.zero);
            Check(demo.GroundPoint(screen,out var world) && world.magnitude<.001f,"Perspective cursor ray meets ground correctly");
            Vector3 ground=demo.View.WorldToScreenPoint(new Vector3(10,3,0));
            Vector3 roof=demo.View.WorldToScreenPoint(new Vector3(10,3,-10));
            Check(Vector2.Distance(ground,roof)>10,"Height produces visible roof displacement");
            demo.ToggleProjection(); ground=demo.View.WorldToScreenPoint(new Vector3(10,3,0)); roof=demo.View.WorldToScreenPoint(new Vector3(10,3,-10));
            Check(!demo.Perspective && Vector2.Distance(ground,roof)<.01f,"Orthographic comparison removes perspective displacement");
            demo.ToggleProjection(); demo.ResetView();
            Check(UnityEngine.Object.FindFirstObjectByType<RogZombie.PreGameplayLoop.LoopSession>()==null,"Visual study does not start or alter gameplay loop");
            double until=EditorApplication.timeSinceStartup+.5; while(EditorApplication.timeSinceStartup<until) yield return null;
        }
    }
}
