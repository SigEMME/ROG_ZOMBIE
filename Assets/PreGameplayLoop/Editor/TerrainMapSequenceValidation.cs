using System;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using RogZombie.PreGameplayLoop;
using ROGZOMBIE.EnvironmentArt;

[InitializeOnLoad]
public static class TerrainMapSequenceValidation
{
    const string Key="TerrainMapSequenceCheck";
    [MenuItem("ROG ZOMBIE/Tests/Verifica sequenza mappe C1")]
    public static void Run() { Directory.CreateDirectory("output"); File.WriteAllText("output/terrain-map-sequence.request", "run"); }
    static IEnumerator checks;
    static double deadline;
    static int warnings;
    static TerrainMapSequenceValidation()
    {
        EditorApplication.update+=Tick;
        EditorApplication.playModeStateChanged+=state=>{
            if(state==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Key,false))
            { checks=Check();deadline=EditorApplication.timeSinceStartup+120; }
        };
        Application.logMessageReceived+=(message,stack,type)=>{
            if(checks==null)return;
            if(type==LogType.Warning)warnings++;
            if(type==LogType.Error||type==LogType.Exception)Finish(false,message+stack);
        };
    }
    static void Tick()
    {
        if(!EditorApplication.isCompiling&&!EditorApplication.isPlayingOrWillChangePlaymode&&File.Exists("output/terrain-map-sequence.request"))
        {
            File.Delete("output/terrain-map-sequence.request");
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if(scene.path!="Assets/Scenes/PreGameplayLoopPrototype.unity"||scene.isDirty)
            {File.WriteAllText("output/terrain-map-sequence-result.txt","BLOCKED: open clean PreGameplayLoopPrototype");return;}
            SessionState.SetBool(Key,true);EditorApplication.isPlaying=true;
        }
        if(checks==null)return;
        try { if(EditorApplication.timeSinceStartup>deadline)throw new Exception("Timeout");if(!checks.MoveNext())Finish(true,"C1_A1 -> C1_A2 -> existing AREA 3 -> restart C1_A1; FIRST SPAWN; navigation; player/gold persistence; atmosphere cleanup; run reset passed. Transitions invoked directly to isolate map integration."); }
        catch(Exception e){Finish(false,e.ToString());}
    }
    static void Require(bool value,string message){if(!value)throw new Exception(message);}
    static IEnumerator Check()
    {
        yield return null;
        var run=UnityEngine.Object.FindFirstObjectByType<LoopSession>();
        Require(run!=null,"No loop session");
        while(run.Loading)yield return null;
        Require(run.State==LoopState.Combat,"Initial map load: "+run.Failure);
        Require(run.Settings.AreaSize == new Vector2(90,90), "C1_A1 size");
        Require(run.Settings.StartPosition == new Vector2(-7,-39), "South road spawn");
        var actor = run.Player.Actor;
        run.AddGold(17);
        Require(run.Navigation.Reachable(run.Settings.StartPosition,run.Definition.TerrainExit),"Navigation");
        var mist=UnityEngine.Object.FindFirstObjectByType<GroundMist>();
        Require(mist&&mist.terrain&&mist.terrain.gameObject.scene==run.gameObject.scene,"Runtime terrain reference");
        Require(mist.GetComponentsInChildren<MeshRenderer>().Length==1,"Fog renderer count");
        var ambient=UnityEngine.Object.FindFirstObjectByType<MapAmbientLighting>();
        Require(ambient&&RenderSettings.sun==ambient.sun,"Map sun");
        var cam=GameObject.Find("Terrain presentation camera").GetComponent<Camera>();
        var data=cam.GetComponent<UniversalAdditionalCameraData>();
        Require(data.renderPostProcessing&&data.volumeLayerMask==(1<<31),"Colour volume visibility");
        ScreenCapture.CaptureScreenshot("output/terrain-map-sequence-preview.png");
        yield return null;yield return null;
        var next=typeof(LoopSession).GetMethod("NextArea",BindingFlags.NonPublic|BindingFlags.Instance);
        run.StartCoroutine((IEnumerator)next.Invoke(run,null));
        yield return null;
        while(run.Loading)yield return null;
        Require(run.AreaIndex==1&&run.State==LoopState.Combat,"AREA 2: "+run.Failure);
        Require(run.Settings.AreaSize == new Vector2(140,140), "C1_A2 size");
        Require(run.Settings.StartPosition == run.Definition.SecondTerrainPlayerStart, "Second map spawn");
        Require(run.Navigation.Reachable(run.Settings.StartPosition,run.Definition.SecondTerrainExit), "Second map exit navigation");
        Require(run.Player.Actor == actor && run.Gold == 17, "Persistent player and gold across maps");
        Require(UnityEngine.Object.FindObjectsByType<GroundMist>(FindObjectsSortMode.None).Length==1,"Second map has exactly one fog");
        var terrain=UnityEngine.Object.FindFirstObjectByType<Terrain>();
        Require(terrain.terrainData==run.Definition.SecondAreaTerrain.GetComponentInChildren<Terrain>().terrainData,"C1_A2 terrain asset");
        ScreenCapture.CaptureScreenshot("output/terrain-map-sequence-area2.png");
        yield return null;
        run.StartCoroutine((IEnumerator)next.Invoke(run,null));yield return null;
        while(run.Loading)yield return null;
        Require(run.AreaIndex==2&&run.State==LoopState.Combat,"AREA 3: "+run.Failure);
        Require(!UnityEngine.Object.FindFirstObjectByType<GroundMist>()&&!UnityEngine.Object.FindFirstObjectByType<MapAmbientLighting>(),"Map atmosphere leaks to third area");
        run.RestartTest();yield return null;
        while(run.Loading)yield return null;
        Require(run.AreaIndex==0&&run.State==LoopState.Combat,"Restart: "+run.Failure);
        Require(UnityEngine.Object.FindObjectsByType<GroundMist>(FindObjectsSortMode.None).Length==1,"Duplicate fog after restart");
        Require(run.Gold==0 && run.Settings.AreaSize==new Vector2(90,90),"Restart resets run and loads C1_A1");
    }
    static void Finish(bool ok,string detail)
    {
        checks=null;SessionState.SetBool(Key,false);
        File.WriteAllText("output/terrain-map-sequence-result.txt",(ok?"PASS":"FAIL")+" warnings="+warnings+"\n"+detail);
        EditorApplication.isPlaying=false;
    }
}
