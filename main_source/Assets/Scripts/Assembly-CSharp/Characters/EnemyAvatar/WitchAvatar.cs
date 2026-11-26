using Bibloteka;
using DarkTonic.MasterAudio;
using System;
using System.Collections;

namespace Characters.EnemyAvatar
{
    // Token: 0x020003BF RID: 959
    public class WitchAvatar : EnemyAvatar
    {
        // Token: 0x06001F94 RID: 8084 RVA: 0x0000857F File Offset: 0x0000677F
        protected override IEnumerator BeforeAttackSoundDelay()
        {
        	yield return null;
        }

        // Token: 0x06001F95 RID: 8085 RVA: 0x00008582 File Offset: 0x00006782
        protected override void EnemySpecificStateChangedBehavior(byte enemySymbol, byte enemyStateSymbol)
        {
        }

        // Token: 0x06001F96 RID: 8086 RVA: 0x00008584 File Offset: 0x00006784
        protected override IEnumerator FadeEffect()
        {
        	yield return Utils.GetDelay(2f);
        	yield return Utils.GetDelay(2f);
        	CustomSingleton<ScreenFader>.Instance.fadeIn = false;
        	yield return Utils.GetDelay(2f);
        }

        // Token: 0x06001F97 RID: 8087 RVA: 0x00008587 File Offset: 0x00006787
        protected override void PlayAttackSound()
        {
        	MasterAudio.PlaySound("Enemy", 1f, null, 0f, "witchShout");
        }
    }
}
