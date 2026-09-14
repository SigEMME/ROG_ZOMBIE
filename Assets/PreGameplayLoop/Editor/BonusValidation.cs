using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using RogZombie.TestEngine;

namespace RogZombie.PreGameplayLoop.Editor
{
    [InitializeOnLoad]
    public static class BonusValidation
    {
        private const string Key = "ROG.Bonus.";
        private static readonly string Request = Path.GetFullPath(".bonus-test.request");
        private static readonly List<string> results = new List<string>();
        private static IEnumerator routine;
        private static double started;
        private static LoopSession loop;
        private static int warnings;
        private static readonly HashSet<string> warningMessages = new HashSet<string>();

        static BonusValidation()
        {
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged += PlayState;
            Application.logMessageReceived += Log;
        }

        [MenuItem("ROG ZOMBIE/Pre gameplay loop/Run BONUS ability tests")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
            if (SceneManager.sceneCount != 1 || SceneManager.GetActiveScene().isDirty || string.IsNullOrEmpty(SceneManager.GetActiveScene().path))
            { Finish(false, "Save the current scene and keep only one scene open before testing."); return; }
            SessionState.SetString(Key + "Previous", SceneManager.GetActiveScene().path);
            EditorSceneManager.OpenScene("Assets/Scenes/PreGameplayLoopPrototype.unity");
            // Select PG08 in memory for this suite only, preserving the user's Inspector choice.
            var config = AssetDatabase.LoadAssetAtPath<LoopDefinition>("Assets/PreGameplayLoop/PreGameplayLoop.asset");
            SessionState.SetInt(Key + "PreviousPlayer", (int)config.SelectedPlayer);
            SessionState.SetBool(Key + "RestoreSelection", true);
            config.SelectedPlayer = LoopPlayer.PG08;
            SessionState.SetBool(Key + "Running", true);
            EditorApplication.isPlaying = true;
        }

        private static void PlayState(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Key + "Running", false))
            {
                results.Clear(); warningMessages.Clear(); warnings = 0; loop = null;
                started = EditorApplication.timeSinceStartup;
                Application.runInBackground = true;
                routine = Flatten(Checks());
            }
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                if (SessionState.GetBool(Key + "RestoreSelection", false))
                {
                    var config = AssetDatabase.LoadAssetAtPath<LoopDefinition>("Assets/PreGameplayLoop/PreGameplayLoop.asset");
                    config.SelectedPlayer = (LoopPlayer)SessionState.GetInt(Key + "PreviousPlayer", 0);
                    SessionState.EraseBool(Key + "RestoreSelection");
                    SessionState.EraseInt(Key + "PreviousPlayer");
                }
                if (SessionState.GetBool(Key + "Running", false)) Finish(false, "Interrupted before completion.");
                string previous = SessionState.GetString(Key + "Previous", "");
                SessionState.EraseString(Key + "Previous");
                if (!string.IsNullOrEmpty(previous)) EditorApplication.delayCall += () => EditorSceneManager.OpenScene(previous);
            }
        }

        private static void Log(string message, string stack, LogType type)
        {
            if (!SessionState.GetBool(Key + "Running", false)) return;
            if (type == LogType.Warning) { warnings++; warningMessages.Add(message); }
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) Finish(false, message + "\n" + stack);
        }

        private static void Tick()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isCompiling && File.Exists(Request))
            {
                if (!SessionState.GetBool(Key + "Refreshed", false))
                {
                    SessionState.SetBool(Key + "Refreshed", true);
                    AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                    return;
                }
                SessionState.EraseBool(Key + "Refreshed");
                SessionState.SetString(Key + "Report", File.ReadAllText(Request).Trim());
                File.Delete(Request);
                Run();
            }
            if (!SessionState.GetBool(Key + "Running", false) || !EditorApplication.isPlaying || routine == null) return;
            try
            {
                if (EditorApplication.timeSinceStartup - started > 180) throw new TimeoutException("Ability test timeout.");
                foreach (var brain in UnityEngine.Object.FindObjectsByType<MobBrain>(FindObjectsSortMode.None)) brain.enabled = false;
                if (loop != null && loop.Player != null)
                {
                    loop.BonusAbilities.enabled = false;
                    var input = loop.Player.GetComponent<PG08AbilityInput>();
                    if (input != null) input.enabled = false;
                    loop.Player.GetComponent<PlayerWeapon>().enabled = false;
                    loop.Player.GetComponent<PlayerMovement>().enabled = false;
                    loop.Player.GetComponent<PlayerAim>().enabled = false;
                }
                if (!routine.MoveNext()) Finish(true, "BONUS abilities, acquisition/caps, combat hooks, EXP and AREA/RUN lifecycle verified in Play Mode.");
            }
            catch (Exception error) { Finish(false, error.ToString()); }
        }

        private static void Check(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
            results.Add("PASS " + message);
            File.WriteAllText(Path.GetFullPath("Builds/BONUS-test-progress.txt"), message);
        }

        private static void Near(float actual, float expected, string message, float tolerance = .001f)
            => Check(Mathf.Abs(actual - expected) <= tolerance, message + $" ({actual:0.###}/{expected:0.###})");

        private static void Finish(bool passed, string detail)
        {
            routine = null;
            SessionState.SetBool(Key + "Running", false);
            string report = SessionState.GetString(Key + "Report", Path.GetFullPath("BONUS-validation.txt"));
            Directory.CreateDirectory(Path.GetDirectoryName(report));
            File.WriteAllText(report, (passed ? "PASS" : "FAIL") + $" | Unity {Application.unityVersion} | checks={results.Count} | warnings={warnings}\n" +
                string.Join("\n", results) + "\n" + detail + "\nWarnings:\n" + string.Join("\n", warningMessages));
            SessionState.EraseString(Key + "Report");
            if (EditorApplication.isPlaying) EditorApplication.isPlaying = false;
        }

        private static Combatant Probe(Vector2 position, Faction faction = Faction.MOB, float defence = 100)
        {
            var go = new GameObject("Ability test actor");
            go.transform.SetParent(loop.transform);
            go.transform.position = position;
            go.layer = LayerMask.NameToLayer(faction == Faction.PG ? "PG" : "MOB");
            go.AddComponent<CircleCollider2D>().radius = .35f;
            var actor = go.AddComponent<Combatant>();
            actor.Initialize(faction, faction == Faction.PG ? CombatStats.FromPG(loop.Player.Definition.BaseStats) : new CombatStats { HP = 500, DEF = defence, ATK = 20, MoveSpeed = 100, Range = 400 });
            return actor;
        }

        private static GameObject Obstacle(Vector2 position, Vector2 size, bool wall)
        {
            var go = new GameObject("Ability test blocker");
            go.transform.SetParent(loop.transform);
            go.transform.position = position;
            go.layer = LayerMask.NameToLayer(wall ? "MURO" : "OSTACOLO");
            go.AddComponent<BoxCollider2D>().size = size;
            go.AddComponent<TestObstacle>().IsWall = wall;
            return go;
        }

        private static readonly Vector2 Origin = new Vector2(30, 30);
        private static IEnumerator Wait(float seconds)
        {
            float end = Time.time + seconds; while (Time.time < end) yield return null;
        }
        private static IEnumerator Fire(Vector2 cursor)
        {
            while (!loop.Player.GetComponent<PlayerWeapon>().TryFireAt(cursor)) yield return null;
        }
        private static void Remove(params Combatant[] actors)
        {
            foreach (var actor in actors) if (actor != null) UnityEngine.Object.Destroy(actor.gameObject);
        }
        private static void HP(Combatant actor, float hp) => actor.SetStats(actor.Stats, hp - actor.CurrentHP);
        private static void ReducedCD() => typeof(LoopSession).GetField("cdReduction", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(loop, 10f);
        private static IEnumerator Flatten(IEnumerator root)
        {
            var stack = new Stack<IEnumerator>(); stack.Push(root);
            while (stack.Count > 0)
            {
                var current = stack.Peek();
                if (!current.MoveNext()) { stack.Pop(); continue; }
                if (current.Current is IEnumerator nested) { stack.Push(nested); continue; }
                yield return current.Current;
            }
        }
        private static BonusAbilityRuntime Fixture(string id, out Combatant actor, out BonusInventory inventory)
        {
            actor = Probe(Vector2.zero, Faction.PG);
            actor.SetStats(new CombatStats { HP = 1000, ATK = 20, DEF = 100, MoveSpeed = 100, AttackSpeed = 100, Range = 400 }, 1000);
            inventory = actor.gameObject.AddComponent<BonusInventory>(); inventory.Catalog = loop.Bonuses.Catalog;
            var runtime = actor.gameObject.AddComponent<BonusAbilityRuntime>(); runtime.Initialize(loop, inventory); runtime.enabled = false;
            inventory.Apply(new BonusChoice(Array.FindIndex(inventory.Catalog.Bonuses, b => b.Id == id), -1));
            return runtime;
        }
        private static void Upgrade(BonusInventory inventory, string id, string key, int times)
        {
            int index = Array.FindIndex(inventory.Catalog.Bonuses, b => b.Id == id);
            int upgrade = Array.FindIndex(inventory.Catalog.Bonuses[index].Upgrades, u => u.Key == key);
            for (int i = 0; i < times; i++) inventory.Apply(new BonusChoice(index, upgrade));
        }
        private static IEnumerator Checks()
        {
            while ((loop = UnityEngine.Object.FindFirstObjectByType<LoopSession>()) == null || loop.State != LoopState.Combat) yield return null;
            Application.runInBackground = true;
            Check(loop.Bonuses != null && loop.Experience != null, "RUN has BONUS inventory and EXP");
            // Isolated combat geometry, without changing scene/asset data.
            foreach (var obstacle in TestObstacle.All.ToArray()) obstacle.gameObject.SetActive(false);
            foreach (var mob in Combatant.All.ToArray()) if (mob.Faction == Faction.MOB) mob.gameObject.SetActive(false);
            loop.Player.transform.position = new Vector2(-30,-30);
            loop.RefreshNavigation();
            var inventory = loop.Bonuses;
            for (int i = 0; i < 100; i++)
            {
                var banners = inventory.Generate();
                Check(banners.Length == 3 && banners[0].Upgrade == -1 && banners[1].Upgrade == -1 && banners[2].Upgrade == -1 &&
                    banners[0].Bonus != banners[1].Bonus && banners[1].Bonus != banners[2].Bonus && banners[0].Bonus != banners[2].Bonus, "Empty slots: three distinct acquisitions " + i);
            }
            var shield = Fixture("shield", out var pg, out var bag);
            Near(shield.Cooldown("shield"),6,"SCUDO acquired in CD");
            Upgrade(bag,"shield","DEF",10); Upgrade(bag,"shield","CD",9);
            Near(shield.Value("shield","DEF"),90,"SCUDO DEF cap90"); Near(shield.Value("shield","CD"),.6f,"SCUDO CD cap90 percent");
            int fallback = 0; bag.ApplyStatFallback = _ => fallback++;
            for (int i=0;i<100;i++) foreach(var banner in bag.Generate())
                Check(banner.Bonus < 0 || banner.Upgrade < 0,"Capped upgrades excluded; STAT fallback or acquisition");
            var statsOnly = bag.Generate(2,new[]{0,1,2});
            foreach(var banner in statsOnly) if(banner.Bonus<0) Check(banner.Upgrade>=3,"Fallback excludes existing endAREA stats");
            shield.Advance(6); Check(shield.ShieldActive,"SCUDO activates");
            var values=pg.Stats;values.DEF=150;pg.SetStats(values);
            pg.Hit(25,true); Near(pg.CurrentHP,999,"SCUDO90 then DEF50 round only final damage (1.25 to1)");
            Check(!shield.ShieldActive,"SCUDO consumes one HIT"); shield.Advance(.6f); shield.ChangeArea(); Check(shield.ShieldActive,"SCUDO survives AREA");
            pg.Hit(100000,true); Check(!shield.ShieldActive,"SCUDO cleared on DOWN"); Remove(pg); yield return null;

            var fire=Fixture("fire",out pg,out bag); var victim=Probe(new Vector2(2,0));
            Near(fire.Cooldown("fire"),12,"FIRE starts in CD"); fire.Advance(12); Check(fire.FireActive,"FIRE active after initial CD");
            victim.Hit(20,true,pg,true); Near(victim.CurrentHP,477,"FIRE BASE HIT20 to23"); Near(pg.Stats.ATK,20,"FIRE does not mutate persistent ATK");
            victim.Hit(35,true,pg); Near(victim.CurrentHP,442,"Fixed ability damage remains35");
            var burn=victim.GetComponent<BonusBurn>(); Check(burn!=null && burn.Stacks==1,"Only BASE HIT applies burn");
            for(int i=0;i<7;i++) victim.Hit(1,true,pg,true);
            Check(burn.Stacks==5,"Burn cap5 and refresh"); float hp=victim.CurrentHP;
            yield return Wait(1.1f); Near(victim.CurrentHP,hp-10,"Burn tick after1 second at5 stacks");
            Upgrade(bag,"fire","BRUCIATURA",1); Near(fire.Value("fire","BRUCIATURA"),2.2f,"Normal DANNO upgrade adds0.2 to burn");
            int fireIndex=bag.Find("fire").DefinitionIndex;bag.Apply(new BonusChoice(fireIndex,1));Near(fire.Value("fire","BRUCIATURA"),3.2f,"Special burn upgrade adds1 from original base");
            values=pg.Stats;values.ATK=21;pg.SetStats(values);fire.Advance(3);Near(pg.Stats.ATK,21,"Persistent upgrade retained after FIRE ends");Near(fire.Cooldown("fire"),12,"FIRE CD starts at end");
            fire.Advance(12);fire.ChangeArea();Check(!fire.FireActive,"FIRE ends on AREA");Near(fire.Cooldown("fire"),12,"FIRE AREA restarts CD");
            Remove(pg,victim);yield return null;

            var ricochet=Fixture("ricochet",out pg,out bag);Upgrade(bag,"ricochet","RIMBALZI",2);
            var a=Probe(new Vector2(0,0));var b=Probe(new Vector2(4,0),Faction.MOB,150);var far=Probe(new Vector2(10,0));
            ricochet.Bounce(a,50);Near(a.CurrentHP,480,"RICOCHET can return to prior MOB");Near(b.CurrentHP,480,"RICOCHET three bounces use original50 before DEF each time");Near(far.CurrentHP,500,"RICOCHET range5 from last target");
            Check(ricochet.RicochetHits==3 && ricochet.RicochetRolls==0,"Additional bounces automatic without rerolls/recursion");
            a.Hit(1,true,pg);Check(ricochet.RicochetRolls==0,"Ability HIT cannot trigger RICOCHET");
            a.Hit(1,true,pg,true);Check(ricochet.RicochetRolls==1,"BASE HIT performs exactly one initial roll");
            Remove(pg,a,b,far);yield return null;

            foreach(string id in new[]{"aura","taser","repulse","slash"})
            {
                var runtime=Fixture(id,out pg,out bag); victim=Probe(new Vector2(1,0));
                if(id=="taser")
                {
                    victim.gameObject.AddComponent<MobBrain>().Initialize(loop.Definition.ZOMB01,loop.Navigation);
                    victim.GetComponent<MobBrain>().enabled=false;
                }
                runtime.Advance(runtime.Cooldown(id));Check(victim.CurrentHP<500,id+" actual area damage");
                if(id=="taser")
                {
                    float until=(float)typeof(MobBrain).GetField("stunnedUntil",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(victim.GetComponent<MobBrain>());
                    Near(until-Time.time,2,"TASER applies actual STUN2 seconds");
                    Near(victim.CurrentHP,loop.Definition.ZOMB01.BaseStats.HP-5,"TASER actual damage5");
                }
                if(id=="repulse") { Vector2 before=victim.transform.position;yield return Wait(.35f);Near(Vector2.Distance(before,victim.transform.position),3,"REPULSE smooth3m",.03f); }
                Remove(pg,victim);yield return null;
            }
            var knives=Fixture("knives",out pg,out bag);Upgrade(bag,"knives","N_COLTELLI",1);
            Near(BonusAbilityRuntime.KnifeAngle(0,2),-2.5f,"Two knives symmetric left");Near(BonusAbilityRuntime.KnifeAngle(1,2),2.5f,"Two knives symmetric right");
            int beforeCount=UnityEngine.Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None).Length;knives.Advance(6);
            Check(UnityEngine.Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None).Length==beforeCount+2,"Two actual knife projectiles emitted");
            foreach(var shot in UnityEngine.Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None))UnityEngine.Object.Destroy(shot.gameObject);
            Remove(pg);yield return null;

            var wall=Obstacle(Vector2.zero,new Vector2(2,2),true);
            Check(BonusMine.TryPosition(Vector2.zero,new Vector2(100,100),out var minePoint),"MINE recalculates occupied position");
            Near(minePoint.magnitude,1,"MINE chooses closest free edge",.004f);
            UnityEngine.Object.Destroy(wall);yield return null;
            wall=Obstacle(Vector2.zero,new Vector2(110,110),true);
            Check(!BonusMine.TryPosition(Vector2.zero,new Vector2(100,100),out _),"MINE no valid point detected");
            var delayedMine=Fixture("mines",out pg,out bag);delayedMine.Advance(7);
            Near(delayedMine.Cooldown("mines"),0,"MINE postponed without restarting CD");
            Check(UnityEngine.Object.FindFirstObjectByType<BonusMine>()==null,"MINE not emitted in blocked AREA");
            Remove(pg);UnityEngine.Object.Destroy(wall);yield return null;
            var mines=Fixture("mines",out pg,out bag);mines.Advance(7);
            var mine=UnityEngine.Object.FindFirstObjectByType<BonusMine>();Check(mine!=null,"MINE launched after CD");Near(mines.Cooldown("mines"),7,"MINE CD starts on launch");
            victim=Probe(mine.transform.position);yield return null;yield return null;Near(victim.CurrentHP,490,"MINE trigger actual damage10");Remove(pg,victim);yield return null;

            var petRuntime=Fixture("pet",out pg,out bag);petRuntime.Advance(.01f);
            var pet=UnityEngine.Object.FindFirstObjectByType<BonusPet>();Check(pet!=null,"PET always active on acquisition");
            victim=Probe(new Vector2(1,0));yield return Wait(.15f);Near(victim.CurrentHP,490,"PET actual attack damage10");
            Remove(victim);petRuntime.ChangeArea();Remove(pg);yield return null;

            // Full gameplay-loop lifecycle: multi-level choices, endAREA acquisition, retained inventory and reset.
            loop.Experience.Award(225);yield return null;yield return null;
            Check(loop.Experience.Level==3 && loop.Experience.PendingChoices==2 && loop.Experience.Experience==5,"Multi-level EXP retains remainder");
            Check(Time.timeScale==0 && loop.Experience.Choices!=null,"LEVEL UP pauses gameplay");
            ScreenCapture.CaptureScreenshot("Builds/BONUS-level-up.png"); yield return null; yield return null;
            loop.Experience.Choose(0);Check(loop.Experience.PendingChoices==1 && Time.timeScale==0,"Multi-level remains paused until last choice");
            loop.Experience.Choose(0);Check(Time.timeScale>0 && loop.Experience.PendingChoices==0,"Last LEVEL UP resumes gameplay");
            int owned=loop.Bonuses.Owned.Count;
            typeof(LoopSession).GetProperty("State").SetValue(loop,LoopState.AreaComplete);
            typeof(LoopSession).GetMethod("OpenBonus",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(loop,null);
            Check(loop.Choices.Length==3 && loop.AbilityChoices.Length==2,"EndAREA has five working banners");
            ScreenCapture.CaptureScreenshot("Builds/BONUS-end-area.png"); yield return null; yield return null;
            loop.SelectBonus(3);loop.ConfirmBonus();
            while(loop.State!=LoopState.Combat)yield return null;
            Check(loop.AreaIndex==1 && loop.Bonuses.Owned.Count>=owned,"AREA transition preserves BONUS inventory");
            Near(loop.Experience.Level,3,"AREA retains LVL");Near(loop.Experience.DropPercent,105,"AREA EXP DROP105 percent");
            loop.RestartTest();while(loop.State!=LoopState.Combat)yield return null;
            Check(loop.Bonuses.Owned.Count==0 && loop.Experience.Level==1 && loop.Experience.Experience==0,"New RUN clears BONUS and EXP");
            var original = loop.Definition;
            var config = UnityEngine.Object.Instantiate(original); loop.Definition = config;
            foreach (LoopPlayer player in Enum.GetValues(typeof(LoopPlayer)))
            {
                config.SelectedPlayer = player; loop.RestartTest();
                while(loop.State!=LoopState.Combat) yield return null;
                foreach(var obstacle in TestObstacle.All.ToArray()) obstacle.gameObject.SetActive(false);
                foreach(var mob in Combatant.All.ToArray()) if(mob.Faction==Faction.MOB) mob.gameObject.SetActive(false);
                loop.Player.transform.position = Vector2.zero;
                loop.Player.GetComponent<PlayerAim>().enabled = false;
                loop.Player.GetComponent<PlayerMovement>().enabled = false;
                loop.Player.GetComponent<PlayerWeapon>().enabled = false;
                loop.BonusAbilities.enabled = false;
                int fireId=Array.FindIndex(loop.Bonuses.Catalog.Bonuses,entry=>entry.Id=="fire");
                int bounceId=Array.FindIndex(loop.Bonuses.Catalog.Bonuses,entry=>entry.Id=="ricochet");
                loop.Bonuses.Apply(new BonusChoice(fireId,-1));loop.Bonuses.Apply(new BonusChoice(bounceId,-1));loop.BonusAbilities.Advance(12);
                victim=Probe(new Vector2(1.5f,0));
                float expected=Mathf.Floor(loop.Player.Actor.EffectiveStats.ATK*1.15f+.5f);
                float hpImmediatelyAfterBase = -1;
                loop.Player.Actor.BaseHitLanded += (hitTarget, damage) => { if(hitTarget==victim) hpImmediatelyAfterBase=hitTarget.CurrentHP; };
                Check(loop.Player.GetComponent<PlayerWeapon>().TryFireAt(new Vector2(1.5f,0)),player+" actual base attack launched");
                float timeout=Time.time+2;
                while(loop.BonusAbilities.RicochetRolls==0 && Time.time<timeout)yield return null;
                Check(loop.BonusAbilities.RicochetRolls==1,player+" BASE HIT triggers exactly one RICOCHET roll");
                Near(hpImmediatelyAfterBase,500-expected,player+" FIRE modifies actual base damage before secondary passive effects");
                Check(victim.GetComponent<BonusBurn>()!=null,player+" actual base HIT applies BRUCIATURA");
                Remove(victim);yield return null;
            }
            loop.Definition=original;loop.RestartTest();while(loop.State!=LoopState.Combat)yield return null;
            UnityEngine.Object.Destroy(config);

        }
    }
}
