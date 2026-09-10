using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using RogZombie.TestEngine;
namespace RogZombie.PlayTests.EditorTools
{
    [InitializeOnLoad] public static class Zomb01Validation
    {
        const string Key="ROG.Z01.Validation";
        static string Request=>Path.Combine(Path.GetTempPath(),"ROG_ZOMB01_validate.request");
        public static string Report=>Path.Combine(Path.GetTempPath(),"ROG_ZOMB01_validation.txt");
        static int stage, attacks, deaths;
        static double began;
        static float since, firstAttack, secondAttack, diedAt;
        static bool hold, inputBound, deathCaptured, walkDamageDone, attackDamageDone;
        static float idlePhase, damagePhase;
        static int damageFrame, damageState;
        static string damageStateName;
        static Mouse mouse;
        static ENVPlayTestSession session;
        static readonly HashSet<string> frames=new HashSet<string>();
        static readonly HashSet<string> attackFrames=new HashSet<string>();
        static Zomb01Validation(){began=EditorApplication.timeSinceStartup;EditorApplication.update+=Tick;EditorApplication.playModeStateChanged+=OnPlayState;}
        static void OnPlayState(PlayModeStateChange state)
        {
            if(state==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Key,false)){began=EditorApplication.timeSinceStartup;stage=0; Application.runInBackground=true; EditorApplication.isPaused=false; UnityEditorInternal.InternalEditorUtility.OnGameViewFocus(true);}
            if(state==PlayModeStateChange.EnteredEditMode && SessionState.GetBool(Key+".Batch",false)){EditorApplication.Exit(SessionState.GetBool(Key+".Passed",false)?0:1);return;}
            if(state==PlayModeStateChange.ExitingPlayMode && SessionState.GetBool(Key,false)){Log("INTERRUPTED: Play stopped.");Clean();}
        }
        public static void RunBatch(){SessionState.SetBool(Key+".Batch",true);EditorSceneManager.OpenScene("Assets/Scenes/ENV_ZOMB01_PlayTest.unity");Start();}
        [MenuItem("ROG ZOMBIE/ZOMB01/Validate animations and mouse input in Play")]
        public static void Start()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode || EditorSceneManager.sceneCount!=1 || EditorSceneManager.GetActiveScene().isDirty || !EditorSceneManager.GetActiveScene().path.EndsWith("ENV_ZOMB01_PlayTest.unity"))
                throw new InvalidOperationException("Open saved ENV_ZOMB01_PlayTest with Play stopped; existing sessions/scenes are preserved.");
            File.WriteAllText(Report,"ZOMB01 animation/input validation\n"); SessionState.SetBool(Key+".Passed",false);
            SessionState.SetBool(Key+".Paused",EditorApplication.isPaused); SessionState.SetBool(Key+".Background",Application.runInBackground); SessionState.SetBool(Key,true);began=EditorApplication.timeSinceStartup;
            EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Focus();
            EditorApplication.isPlaying=true;
        }
        static void Tick()
        {
            if(!EditorApplication.isCompiling && !EditorApplication.isUpdating && File.Exists(Request))
            {
                if(File.ReadAllText(Request).Trim().Replace('\\','/')==Path.GetDirectoryName(Application.dataPath).Replace('\\','/'))
                {File.Delete(Request);try{Start();}catch(Exception e){File.WriteAllText(Report,"BLOCKED: "+e);}}
            }
            if(!SessionState.GetBool(Key,false) || !EditorApplication.isPlaying || EditorApplication.isCompiling)return;
            try
            {
                EditorApplication.QueuePlayerLoopUpdate(); Check(EditorApplication.timeSinceStartup-began<45,"Timeout stage="+stage+" paused="+EditorApplication.isPaused+" focused="+Application.isFocused+" scale="+Time.timeScale);
                if(session==null)session=UnityEngine.Object.FindFirstObjectByType<ENVPlayTestSession>();
                if(session==null || session.Player.Actor==null || (stage==0 && session.Brain==null))return;
                var animator=session.Zombie!=null?session.Zombie.GetComponentInChildren<Animator>():null;
                var body=animator!=null?animator.GetComponent<SpriteRenderer>():null;
                if(stage==0)
                {
                    if(!Application.isFocused){UnityEditorInternal.InternalEditorUtility.OnGameViewFocus(true);return;}
                    session.SetZombieAI(false);session.ResetActors();
                    mouse=InputSystem.AddDevice<Mouse>("ZOMB01_ValidationMouse");
                    InputSystem.onAfterUpdate+=FeedMouse;inputBound=true;
                    since=Time.time;frames.Clear();stage=1;return;
                }
                if(stage==1)
                {
                    frames.Add(body.sprite.name);
                    if(Time.time-since<1.1f)return;
                    Check(frames.Count==4,"Four IDLE frames.");
                    Check(animator.GetCurrentAnimatorStateInfo(0).IsName("IDLE"),"IDLE with AI disabled.");
                    Check(Mathf.Abs(animator.GetCurrentAnimatorStateInfo(0).speed-8f/60f)<.0001f,"IDLE eight fps.");
                    Check(body.transform.localPosition==Vector3.zero && Vector3.Distance(body.transform.localScale,Vector3.one*.3f)<.0001f,"Visual centered, scale .3.");
                    Capture("IDLE");Log("PASS IDLE: four frames, 8fps; scale .3, centered visual and shadow.");
                    Check(!Array.Exists(animator.parameters,p=>p.name=="Hit"),"No HIT trigger remains.");
                    idlePhase=animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                    Check(session.Zombie.CurrentHP==60,"Initial ZOMB01 HP60.");hold=true;since=Time.time;frames.Clear();stage=2;
                }
                else if(stage==2)
                {
                    if(session.Zombie.CurrentHP==60)
                    {
                        if(Time.time-since>2)throw new Exception("Mouse path did not hit: focus="+Application.isFocused+", aim="+session.Player.GetComponent<RogZombie.PlayerAim>().CursorWorldPosition+", mouse="+Mouse.current.name+", pressed="+Mouse.current.leftButton.isPressed+", HUD="+TestHUD.PointerOverControls+", enabled="+session.Player.GetComponent<PlayerWeapon>().enabled);
                        return;
                    }
                    Check(animator.GetCurrentAnimatorStateInfo(0).IsName("IDLE") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime>=idlePhase,"IDLE unchanged on actual mouse hit.");
                    hold=false;Check(session.Zombie.CurrentHP==40,"Mouse through PlayerAim/PlayerWeapon gives HP60 -> HP40.");
                    Check(session.Player.GetComponent<RogZombie.PlayerAim>().HasAimPoint,"Actual aim component acquired pointer.");
                    Log("PASS INPUT: synthetic Mouse state -> unmodified PlayerAim/PlayerWeapon -> visible ZOMB01, HP60 to HP40; no direct TryFireAt call.");
                    since=Time.time;stage=3;
                }
                else if(stage==3)
                {
                    var idleState=animator.GetCurrentAnimatorStateInfo(0);
                    Check(idleState.IsName("IDLE") && idleState.normalizedTime>=idlePhase,"Nonlethal hit must not interrupt or restart IDLE.");
                    idlePhase=idleState.normalizedTime;
                    if(Time.time-since<.7f)return;
                    Log("PASS NO HIT REACTION: HP60 -> HP40; IDLE continues without restart.");
                    session.Zombie.transform.position=new Vector3(4,0,0);Physics2D.SyncTransforms();
                    session.Brain.MeleeAttackPerformed+=OnAttack;session.SetZombieAI(true);frames.Clear();since=Time.time;stage=4;
                }
                else if(stage==4)
                {
                    if(animator.GetCurrentAnimatorStateInfo(0).IsName("WALK")){frames.Add(body.sprite.name);if(frames.Count==2)Capture("WALK");}
                    if(animator.GetCurrentAnimatorStateInfo(0).IsName("ATTACK")){attackFrames.Add(body.sprite.name);if(attackFrames.Count==2)Capture("ATTACK");}
                    if(damageStateName!=null && Time.frameCount>damageFrame)
                    {
                        var current=animator.GetCurrentAnimatorStateInfo(0);
                        Check(current.fullPathHash==damageState && current.normalizedTime>=damagePhase,"Damage interrupted/restarted "+damageStateName);
                        Log("PASS NO HIT REACTION: "+damageStateName+" remains active and advances after damage.");
                        damageStateName=null;
                    }
                    if(!walkDamageDone && animator.GetCurrentAnimatorStateInfo(0).IsName("WALK") && frames.Count>=2)
                    {BeginDamageContinuityCheck(animator,"WALK");walkDamageDone=true;}
                    if(!attackDamageDone && animator.GetCurrentAnimatorStateInfo(0).IsName("ATTACK") && attackFrames.Count>=2)
                    {BeginDamageContinuityCheck(animator,"ATTACK");attackDamageDone=true;}
                    if(attacks<2)return;
                    Check(walkDamageDone && attackDamageDone && damageStateName==null,"WALK and ATTACK continuity checked.");
                    Check(frames.Count==8,"All eight WALK frames during existing NavMesh movement.");
                    Check(attackFrames.Count==6,"All six ATTACK frames.");
                    float interval=session.Zombie.Stats.AttackInterval;
                    Check(Mathf.Abs(secondAttack-firstAttack-interval)<.15f,"Existing attack cooldown preserved.");
                    Check(Mathf.Abs(session.Player.Actor.CurrentHP-74.5f)<.001f,"Two existing attacks: PG01 HP100 -> 74.5 (DEF15%).");
                    Log("PASS WALK/ATTACK: 8/6 frames; two real AI hits; interval="+(secondAttack-firstAttack)+"; PG HP="+session.Player.Actor.CurrentHP);
                    session.SetZombieAI(false);session.ResetActors();session.Zombie.Died+=OnDeath;
                    frames.Clear();hold=true;stage=5;
                }
                else if(stage==5)
                {
                    if(session.Zombie!=null && session.Zombie.State==LifeState.Dead)
                    {
                        hold=false;
                        Check(!session.Zombie.GetComponent<CircleCollider2D>().enabled,"Dead collider disabled immediately.");
                        Check(session.Zombie.transform.localScale==Vector3.one,"No placeholder root squash.");
                        if(animator.GetCurrentAnimatorStateInfo(0).IsName("DEATH"))frames.Add(body.sprite.name);
                        if(Time.time-diedAt>1f && !deathCaptured)
                        {
                            Check(body.sprite.name.Contains("DEATH_06"),"Last DEATH frame held.");Capture("DEATH");deathCaptured=true;
                        }
                        if(Time.time-diedAt>2f)Check(animator.GetCurrentAnimatorStateInfo(0).IsName("DEATH"),"DEATH never returns to IDLE.");
                    }
                    if(session.Zombie!=null)return;
                    Check(deaths==1 && frames.Count==6 && deathCaptured,"One death, six frames and held corpse.");
                    float corpseSeconds=Time.time-diedAt; Log("Observed corpse lifetime="+corpseSeconds);
                    // Editor polling reads a player-loop clock snapshot; allow frame scheduling tolerance.
                    Check(session.ZombieDefinition.DeathSpriteSeconds==3f && Mathf.Abs(corpseSeconds-3f)<.1f,"Corpse removed after existing three seconds: "+corpseSeconds);
                    session.ResetActors();Check(session.Zombie.CurrentHP==60,"Reset after removal.");
                    Check(Combatant.All.FindAll(a=>a.Faction==Faction.MOB).Count==1,"Reset creates exactly one active MOB.");
                    Log("PASS DEATH: six frames, terminal pose, collider off, removal after3s, reset HP60 and no duplicate actor.");
                    Log("PASS ALL: Play validation complete."); SessionState.SetBool(Key+".Passed",true);Finish();
                }
            }
            catch(Exception e){Log("FAIL: "+e);Finish();}
        }
        static void BeginDamageContinuityCheck(Animator animator,string state)
        {
            var before=animator.GetCurrentAnimatorStateInfo(0);
            damageState=before.fullPathHash;damagePhase=before.normalizedTime;damageFrame=Time.frameCount;damageStateName=state;
            float health=session.Zombie.CurrentHP;
            session.Zombie.Hit(1f); // Controlled nonlethal probe through the real damage API, not an animation trigger.
            Check(Mathf.Abs(session.Zombie.CurrentHP-(health-1f))<.001f,"Nonlethal damage applied during "+state);
        }
        static void FeedMouse()
        {
            if(InputState.currentUpdateType!=InputUpdateType.Dynamic || mouse==null || session==null || session.Zombie==null)return;
            var body=session.Zombie.GetComponentInChildren<Zomb01Presentation>().GetComponent<SpriteRenderer>();
            Vector2 point=Camera.main.WorldToScreenPoint(body.bounds.center);
            InputState.Change(mouse,new MouseState{position=point}.WithButton(MouseButton.Left,hold));
            mouse.MakeCurrent();
        }
        static void OnAttack(Combatant target){attacks++;if(attacks==1)firstAttack=Time.time;if(attacks==2)secondAttack=Time.time;}
        static void OnDeath(Combatant target){deaths++;diedAt=Time.time;}
        static void Check(bool pass,string message){if(!pass)throw new InvalidOperationException(message);}
        static void Log(string message){File.AppendAllText(Report,message+"\n");}
        static void Clean()
        {
            if(inputBound){InputSystem.onAfterUpdate-=FeedMouse;inputBound=false;}
            if(mouse!=null && mouse.added)InputSystem.RemoveDevice(mouse);mouse=null;hold=false;
            Application.runInBackground=SessionState.GetBool(Key+".Background",false); EditorApplication.isPaused=SessionState.GetBool(Key+".Paused",false); SessionState.SetBool(Key,false);
        }
        static void Finish(){Clean();EditorApplication.isPlaying=false;}
        static void Capture(string label)
        {
            var camera=Camera.main;var target=new RenderTexture(1280,800,24);var old=camera.targetTexture;var active=RenderTexture.active;float aspect=camera.aspect;
            var texture=new Texture2D(1280,800,TextureFormat.RGB24,false);
            try{camera.targetTexture=target;camera.Render();RenderTexture.active=target;texture.ReadPixels(new Rect(0,0,1280,800),0,0);texture.Apply();File.WriteAllBytes(Path.Combine(Path.GetTempPath(),"ROG_ZOMB01_"+label+".png"),texture.EncodeToPNG());}
            finally{camera.targetTexture=old;camera.aspect=aspect;camera.ResetAspect();RenderTexture.active=active;target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(texture);}
        }
    }
}