using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using RogZombie.TestEngine;
namespace RogZombie.PlayTests.EditorTools
{
    // Explicit menu/tool requests only; importing or opening Unity never starts a test.
    [InitializeOnLoad]
    public static class Zomb01PlayTestTools
    {
        const string Art="Assets/Art/MOB/ZOMB01/";
        const string Scene="Assets/Scenes/ENV_ZOMB01_PlayTest.unity";
        static readonly string Request=Path.Combine(Path.GetTempPath(),"ROG_ZOMB01_build.request");
        public static string Report => Path.Combine(Path.GetTempPath(),"ROG_ZOMB01_build_report.txt");
        static Zomb01PlayTestTools(){EditorApplication.update+=Poll;}
        static void Poll()
        {
            if(EditorApplication.isCompiling || EditorApplication.isUpdating || !File.Exists(Request))return;
            if(File.ReadAllText(Request).Trim().Replace('\\','/') != Path.GetDirectoryName(Application.dataPath).Replace('\\','/'))return;
            File.Delete(Request);
            try{Build();}catch(Exception e){File.WriteAllText(Report,"BLOCKED/FAILED: "+e);Debug.LogException(e);}
        }
        [MenuItem("ROG ZOMBIE/ZOMB01/Build animation assets and test scene")]
        public static void Build()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play before building ZOMB01 assets.");
            if(EditorSceneManager.sceneCount!=1 || EditorSceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Preserving open/unsaved scenes; save before building.");
            var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(Art+"ZOMB01.controller");
            var sm=controller.layers[0].stateMachine;
            AddParameter(controller,"Moving",AnimatorControllerParameterType.Bool);
            AddParameter(controller,"Dead",AnimatorControllerParameterType.Bool);
            AddParameter(controller,"Attack",AnimatorControllerParameterType.Trigger);
            AddParameter(controller,"AttackPlayback",AnimatorControllerParameterType.Float);
            var idle=GetState(sm,"IDLE"); idle.speed=8f/60f;
            var walk=GetState(sm,"WALK"); walk.motion=MakeClip("WALK",8,true);
            var attack=GetState(sm,"ATTACK"); attack.motion=MakeClip("ATTACK",6,false);
            var death=GetState(sm,"DEATH"); death.motion=MakeClip("DEATH",6,false);
            attack.speedParameter="AttackPlayback"; attack.speedParameterActive=true;
            foreach(var state in new[]{idle,walk,attack,death})
                foreach(var t in state.transitions.Where(t=>t.name.StartsWith("Z01_")).ToArray())state.RemoveTransition(t);
            foreach(var t in sm.anyStateTransitions.Where(t=>t.name.StartsWith("Z01_")).ToArray())sm.RemoveAnyStateTransition(t);
            var deathTransition=sm.AddAnyStateTransition(death); Configure(deathTransition,"Death"); deathTransition.canTransitionToSelf=false; deathTransition.AddCondition(AnimatorConditionMode.If,0,"Dead");
            var attackTransition=sm.AddAnyStateTransition(attack); Configure(attackTransition,"Attack"); attackTransition.canTransitionToSelf=true; attackTransition.AddCondition(AnimatorConditionMode.If,0,"Attack"); attackTransition.AddCondition(AnimatorConditionMode.IfNot,0,"Dead");
            var moving=idle.AddTransition(walk); Configure(moving,"Walk"); moving.AddCondition(AnimatorConditionMode.If,0,"Moving");
            var stopped=walk.AddTransition(idle); Configure(stopped,"Idle"); stopped.AddCondition(AnimatorConditionMode.IfNot,0,"Moving");
            foreach(var state in new[]{attack})
                foreach(var dest in new[]{walk,idle})
                {
                    var t=state.AddTransition(dest); Configure(t,state.name+dest.name); t.hasExitTime=true; t.exitTime=1f;
                    t.AddCondition(dest==walk?AnimatorConditionMode.If:AnimatorConditionMode.IfNot,0,"Moving");
                }
            sm.defaultState=idle;
            EditorUtility.SetDirty(controller); AssetDatabase.SaveAssetIfDirty(controller);
            var scene=EditorSceneManager.GetActiveScene().path==Scene?EditorSceneManager.GetActiveScene():EditorSceneManager.OpenScene(Scene);
            var session=UnityEngine.Object.FindFirstObjectByType<ENVPlayTestSession>();
            if(session==null || session.Zombie==null)throw new InvalidOperationException("Scene references missing.");
            var brain=session.Zombie.GetComponent<MobBrain>(); if(brain==null)brain=session.Zombie.gameObject.AddComponent<MobBrain>();
            brain.Definition=session.ZombieDefinition; brain.HasAnimatedPresentation=true;
            var animator=session.Zombie.GetComponentInChildren<Animator>();
            animator.runtimeAnimatorController=controller; animator.applyRootMotion=false;
            animator.transform.localPosition=Vector3.zero; animator.transform.localScale=Vector3.one*.3f;
            animator.gameObject.name="ZOMB01 Visual";
            var presenter=animator.GetComponent<Zomb01Presentation>();if(presenter==null)presenter=animator.gameObject.AddComponent<Zomb01Presentation>();
            presenter.AttackClip=(AnimationClip)attack.motion;
            var shadow=session.Zombie.transform.Find("ZOMB01 Shadow");
            if(shadow==null){shadow=new GameObject("ZOMB01 Shadow").transform; shadow.SetParent(session.Zombie.transform,false);shadow.gameObject.AddComponent<SpriteRenderer>();}
            shadow.localPosition=new Vector3(0,-.45f,.01f);shadow.localScale=Vector3.one*.3f;
            var sr=shadow.GetComponent<SpriteRenderer>();sr.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Art+"SHADOW/ZOMB01_SHADOW.png");
            var body=animator.GetComponent<SpriteRenderer>();sr.sharedMaterial=body.sharedMaterial;sr.sortingLayerID=body.sortingLayerID;sr.sortingOrder=body.sortingOrder-1;
            session.RunZombieAI=true;
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            File.WriteAllText(Report,"PASS: three action/movement clips, IDLE8fps, four Animator states and scene wiring saved. PNG/importers and original IDLE clip preserved.\n");
        }
        static void AddParameter(AnimatorController c,string name,AnimatorControllerParameterType type){if(!c.parameters.Any(p=>p.name==name))c.AddParameter(name,type);}
        static AnimatorState GetState(AnimatorStateMachine sm,string name){return sm.states.Select(s=>s.state).FirstOrDefault(s=>s.name==name) ?? sm.AddState(name);}
        static void Configure(AnimatorStateTransition t,string name){t.name="Z01_"+name;t.duration=0;t.hasExitTime=false;}
        static AnimationClip MakeClip(string kind,int count,bool loop)
        {
            string path=Art+"ZOMB01_"+kind+".anim";
            var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if(clip==null){clip=new AnimationClip();AssetDatabase.CreateAsset(clip,path);}
            clip.frameRate=8;
            var keys=new ObjectReferenceKeyframe[count+1];
            for(int i=0;i<count;i++)
            {
                var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Art+kind+"/ZOMB01_"+kind+"_"+(i+1).ToString("00")+".png");
                if(sprite==null)throw new InvalidOperationException("Missing sprite "+kind+" "+i);
                keys[i]=new ObjectReferenceKeyframe{time=i/8f,value=sprite};
            }
            keys[count]=new ObjectReferenceKeyframe{time=count/8f,value=loop?keys[0].value:keys[count-1].value};
            AnimationUtility.SetObjectReferenceCurve(clip,EditorCurveBinding.PPtrCurve("",typeof(SpriteRenderer),"m_Sprite"),keys);
            var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=loop;AnimationUtility.SetAnimationClipSettings(clip,settings);
            EditorUtility.SetDirty(clip);AssetDatabase.SaveAssetIfDirty(clip);return clip;
        }
    }
}