using Bibloteka;
using System;
using System.Collections;
using Characters.Agents;
using Characters.States;

namespace Characters.Strategies
{
    // Token: 0x02000384 RID: 900
    public class CharlieAttackStrategy : SimpleAttackStrategy
    {
        // Token: 0x06001E30 RID: 7728 RVA: 0x0000806F File Offset: 0x0000626F
        public CharlieAttackStrategy(EnemyAgent enemyAgent) : base(enemyAgent)
        {
        }

        // Token: 0x06001E31 RID: 7729 RVA: 0x00008078 File Offset: 0x00006278
        protected override IEnumerator FadeEffect()
        {
        	yield return Utils.GetDelay(1f);
        	yield return Utils.GetDelay(1f);
        	CustomSingleton<ScreenFader>.Instance.fadeIn = false;
        	yield return Utils.GetDelay(2f);
        }

        // Token: 0x06001E32 RID: 7730 RVA: 0x0000807B File Offset: 0x0000627B
        public override IEnumerator Start(EnemyStateController stateController)
        {
        	enemyAgent.Attack(GetAttackedTarget());
        	yield return Utils.GetDelay(0.3f);
        	enemyAgent.PlayAttackSound();
        	yield return FadeEffect();
        	stateController.StartNewState(stateController.InactiveState);
        }
    }
}
