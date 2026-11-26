using Bibloteka;
using DarkTonic.MasterAudio;
using System;
using System.Collections;

namespace Characters.EnemyAvatar
{
    // Token: 0x020003BE RID: 958
    public class KrasueAvatar : EnemyAvatar
    {
        // Token: 0x06001F8F RID: 8079 RVA: 0x0000856D File Offset: 0x0000676D
        protected override IEnumerator BeforeAttackSoundDelay()
        {
        	yield return null;
        }

        // Token: 0x06001F90 RID: 8080 RVA: 0x00008570 File Offset: 0x00006770
        protected override void PlayAttackSound()
        {
        	MasterAudio.PlaySound("Enemy", 1f, null, 0f, "shout");
        }

        // Token: 0x06001F91 RID: 8081 RVA: 0x00008572 File Offset: 0x00006772
        protected override IEnumerator FadeEffect()
        {
        	yield return Utils.GetDelay(2f);
        	CustomSingleton<ScreenFader>.Instance.fadeIn = false;
        }

        // Token: 0x06001F92 RID: 8082 RVA: 0x00008575 File Offset: 0x00006775
        protected override void EnemySpecificStateChangedBehavior(byte enemySymbol, byte enemyStateSymbol)
        {
        }
    }
}
