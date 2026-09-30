using System.Collections;
using UnityEngine;
using RogZombie.TestEngine;
using RogZombie.BossTest;
namespace RogZombie.PreGameplayLoop
{
    public sealed partial class LoopSession
    {
        // Isolated test: start at the end of AREA 3, reuse the real exit/party/bonus
        // transition, then build AREA 4 without invoking the ordinary MOB spawner.
        private IEnumerator BuildBossTestStage(BossTestScene test)
        {
            if (!test.InBossArea)
            {
                var exitObject = TestVisuals.Box("USCITA AREA 3 → BOSS", Definition.ExitForArea(AreaIndex), Vector2.one, Color.green, 1);
                Exit = exitObject.AddComponent<AreaExitTrigger>();
                Exit.Initialize(Player.Actor);
                Exit.Entered += OpenBonus;
                Exit.PartyReady = PartyReadyForExit;
                Exit.Open();
                State = LoopState.AreaComplete;
                Time.timeScale = 1;
                yield break;
            }
            State = LoopState.Transition;
            Time.timeScale = 0;
            test.Begin(areaRoot);
            yield return test.PlayEntrance();
            State = LoopState.Combat;
            Time.timeScale = 1;
        }
    }
}
