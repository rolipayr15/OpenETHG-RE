using DarkTonic.MasterAudio;
using Bibloteka;
using System;
using System.Collections;
using Characters.Agents;

namespace Characters.Strategies
{
    // Token: 0x02000387 RID: 903
    public class DogAttackStrategy : SimpleAttackStrategy
    {
        // Token: 0x06001E3A RID: 7738 RVA: 0x0000809A File Offset: 0x0000629A
        public DogAttackStrategy(EnemyAgent enemyAgent) : base(enemyAgent)
        {
        }

        // Token: 0x06001E3B RID: 7739 RVA: 0x000080A3 File Offset: 0x000062A3
        protected override IEnumerator FadeEffect()
        {
        	yield return Utils.GetDelay(1.5f);
        	CustomSingleton<ScreenFader>.Instance.fadeIn = false;
        	yield return Utils.GetDelay(0.4f);
        	MasterAudio.PlaySound3DAtTransform("music", CustomSingleton<PlayerManager>.Instance.Player.transform);
        }
    }
}
