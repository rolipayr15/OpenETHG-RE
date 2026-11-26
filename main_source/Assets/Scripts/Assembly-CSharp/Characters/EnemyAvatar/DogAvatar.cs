using UnityEngine;
using Bibloteka;
using DarkTonic.MasterAudio;
using System;
using System.Collections;

namespace Characters.EnemyAvatar
{
    // Token: 0x020003BB RID: 955
    public class DogAvatar : EnemyAvatar
    {
        // Token: 0x06001F75 RID: 8053 RVA: 0x00008529 File Offset: 0x00006729
        protected override IEnumerator BeforeAttackSoundDelay()
        {
        	yield return null;
        }

        // Token: 0x06001F76 RID: 8054 RVA: 0x0000852C File Offset: 0x0000672C
        protected override void EnemySpecificStateChangedBehavior(byte enemySymbol, byte enemyStateSymbol)
        {
        	if (enemySymbol != 'D')
        	{
        		return;
        	}
        	switch ((char)enemyStateSymbol)
        	{
        	case 'C':
        		anim.SetFloat("agent_speed", chaseSpeed);
        		Debug.Log("Set anim trigger to run and speed to " + chaseSpeed);
        		anim.SetTrigger("run");
        		break;
        	case 'E':
        		MasterAudio.PlaySound3DAtTransform("DogFx", CustomSingleton<PlayerManager>.Instance.Player.transform, 1f, null, 0f, "eat");
        		anim.SetFloat("agent_speed", 0f);
        		anim.SetTrigger("eat");
        		break;
        	case 'L':
        		anim.SetFloat("agent_speed", patrolSpeed);
        		anim.SetTrigger("walk");
        		break;
        	case 'P':
        		anim.SetFloat("agent_speed", patrolSpeed);
        		Debug.Log("Set anim trigger to walk and speed to " + patrolSpeed);
        		anim.SetTrigger("walk");
        		break;
        	}
        }

        // Token: 0x06001F77 RID: 8055 RVA: 0x0000852E File Offset: 0x0000672E
        protected override IEnumerator FadeEffect()
        {
        	yield return Utils.GetDelay(0.5f);
        	CustomSingleton<ScreenFader>.Instance.fadeIn = false;
        	yield return Utils.GetDelay(0.4f);
        }

        // Token: 0x06001F78 RID: 8056 RVA: 0x00008531 File Offset: 0x00006731
        protected override void PlayAttackSound()
        {
        	MasterAudio.PlaySound("DogFx", 1f, null, 0f, "attack");
        }
    }
}
