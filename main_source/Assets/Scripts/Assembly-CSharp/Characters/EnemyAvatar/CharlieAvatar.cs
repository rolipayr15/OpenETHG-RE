using UnityEngine;
using Bibloteka;
using DarkTonic.MasterAudio;
using Prime31.MessageKit;
using System;
using System.Collections;

namespace Characters.EnemyAvatar
{
    // Token: 0x020003BA RID: 954
    public class CharlieAvatar : EnemyAvatar
    {
        // Token: 0x06001F6B RID: 8043 RVA: 0x0000850C File Offset: 0x0000670C
        protected override void Start()
        {
        	SetCharlieTransparency(1f);
        	MessageKit<bool, float>.addObserver(118, AnimatePortalTravel);
        	base.Start();
        }

        // Token: 0x06001F6C RID: 8044 RVA: 0x0000850E File Offset: 0x0000670E
        protected override void OnDestroy()
        {
        	MessageKit<bool, float>.removeObserver(118, AnimatePortalTravel);
        	base.OnDestroy();
        }

        // Token: 0x06001F6D RID: 8045 RVA: 0x00008510 File Offset: 0x00006710
        public void SetCharlieTransparency(float a)
        {
        	Shader.SetGlobalFloat("_CharlieTransparency", baseTransparency * a);
        	Shader.SetGlobalFloat("_CharliePortalFade", a);
        }

        // Token: 0x06001F6E RID: 8046 RVA: 0x00008512 File Offset: 0x00006712
        private IEnumerator UpdateCharlieTransparencyDuringPortalTravel(bool travelIntoPortal, float duration)
        {
        	float t = 0f;
        	while (t <= duration)
        	{
        		float fraction = t / duration;
        		SetCharlieTransparency(travelIntoPortal ? 1f - fraction : fraction);
        		t += Time.deltaTime;
        		yield return null;
        	}
        	SetCharlieTransparency(travelIntoPortal ? 0f : 1f);
        }

        // Token: 0x06001F6F RID: 8047 RVA: 0x00008515 File Offset: 0x00006715
        private void AnimatePortalTravel(bool travelIntoPortal, float duration)
        {
        	Debug.Log("Start charlie avatar transparency update. Travel into portal = " + travelIntoPortal);
        	StartCoroutine(UpdateCharlieTransparencyDuringPortalTravel(travelIntoPortal, duration));
        }

        // Token: 0x06001F70 RID: 8048 RVA: 0x00008517 File Offset: 0x00006717
        protected override IEnumerator BeforeAttackSoundDelay()
        {
        	yield return Utils.GetDelay(0.3f);
        }

        // Token: 0x06001F71 RID: 8049 RVA: 0x0000851A File Offset: 0x0000671A
        protected override void EnemySpecificStateChangedBehavior(byte enemySymbol, byte enemyStateSymbol)
        {
        }

        // Token: 0x06001F72 RID: 8050 RVA: 0x0000851C File Offset: 0x0000671C
        protected override IEnumerator FadeEffect()
        {
        	yield return Utils.GetDelay(1f);
        	yield return Utils.GetDelay(1f);
        	CustomSingleton<ScreenFader>.Instance.fadeIn = false;
        	yield return Utils.GetDelay(2f);
        }

        // Token: 0x06001F73 RID: 8051 RVA: 0x0000851F File Offset: 0x0000671F
        protected override void PlayAttackSound()
        {
        	MasterAudio.PlaySound("Enemy", 1f, null, 0f, "charlieShout");
        }

        // Token: 0x04001AD5 RID: 6869
        private float baseTransparency = 0.6f;
    }
}
